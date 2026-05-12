using System.Text;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using MTG_Emulator.Backend.DB;
using MTG_Emulator.Backend.DB.Models;
using MTG_Emulator.Backend.Scalar;
using MTG_Emulator.Backend.Scryfall;
using Scalar.AspNetCore;
using Serilog;

namespace MTG_Emulator.Backend
{
    internal abstract class Program
    {
        public static async Task Main(string[] args)
        {
            Env.Load();
            var builder = WebApplication.CreateBuilder(args);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            builder.Host.UseSerilog();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
            });

            if (builder.Environment.IsDevelopment())
                builder.Configuration.AddUserSecrets<Program>();

            builder.Services.AddDbContext<MTGContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions => sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 10,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null))
                       .EnableSensitiveDataLogging()
                       .EnableDetailedErrors()
                       .LogTo(Log.Information, LogLevel.Warning));

            builder.Services.AddIdentity<ApiUser, ApiRole>(options =>
                {
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireNonAlphanumeric = true;
                    options.Password.RequiredLength = 8;
                })
                .AddEntityFrameworkStores<MTGContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    options.DefaultChallengeScheme =
                        options.DefaultForbidScheme =
                            options.DefaultScheme =
                                options.DefaultSignInScheme =
                                    options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["JWT:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["JWT:Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            builder.Configuration["JWT:SigningKey"]
                            ?? throw new InvalidOperationException("JWT:SigningKey is not configured.")))
                };
            });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy =>
                    policy.RequireRole(Roles.Admin));

                options.AddPolicy("PlayerOrAdmin", policy =>
                    policy.RequireRole(Roles.Player, Roles.Admin));
            });

            builder.Services.AddHttpLogging(logging =>
            {
                logging.LoggingFields = HttpLoggingFields.All;
                logging.RequestHeaders.Add("User-Agent");
                logging.ResponseHeaders.Add("MyResponseHeader");
                logging.RequestBodyLogLimit = 4096;
                logging.ResponseBodyLogLimit = 4096;
                logging.CombineLogs = true;
            });

            var app = builder.Build();

            var dataRoot = Environment.GetEnvironmentVariable("SCRYFALL_DATA_PATH")
                ?? Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "scryfall-data");

            var imagesPath = Path.Combine(dataRoot, "images");
            Directory.CreateDirectory(imagesPath);

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.GetFullPath(imagesPath)),
                RequestPath = "/cards",
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers.CacheControl = "public,max-age=31536000,immutable";
                }
            });

            app.UseSerilogRequestLogging();
            app.UseHttpLogging();

            // Middleware order matters — auth must come before MapControllers and authentication must come before authorization
            app.UseAuthentication();
            app.UseAuthorization();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<MTGContext>();

                try
                {
                    // For some (currently undecipherable) reason migration attempts fail
                    // on composing up, but retrying succeeds ???
                    await db.Database.MigrateAsync();
                }
                catch (SqlException ex) when (ex.Number == 1801)
                {
                    Log.Warning("Database already exists, skipping creation — applying pending migrations.");
                    await db.Database.MigrateAsync();
                }

                await ScryfallImageDownloader.RunAsync(testMode: false);

                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApiRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApiUser>>();
                
                await RoleSeeder.SeedRolesAsync(roleManager);
                await AdminSeeder.SeedAdminAsync(userManager);
                
                await DbHelper.SeedDb(db, userManager);
            }

            app.MapControllers();
            app.MapOpenApi();
            app.MapScalarApiReference();

            app.Run();
        }
    }
}