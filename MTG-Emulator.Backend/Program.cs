using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.FileProviders;
using MTG_Emulator.Backend.DB;
using Scalar.AspNetCore;
using Serilog;

namespace MTG_Emulator.Backend
{
    internal abstract class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            builder.Host.UseSerilog();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddOpenApi();

            builder.Services.AddDbContext<MTGContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions => sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 10,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null))
                       .EnableSensitiveDataLogging()
                       .EnableDetailedErrors()
                       .LogTo(Log.Information, Microsoft.Extensions.Logging.LogLevel.Warning));

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

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<MTGContext>();
                var dbCreator = db.Database.GetService<IRelationalDatabaseCreator>();

                try
                {
                    await dbCreator.CreateAsync();
                    Log.Information("Database created.");
                }
                catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 1801)
                {
                    Log.Information("Database already exists, continuing.");
                }

                var strategy = db.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    var pendingMigrations = await db.Database.GetPendingMigrationsAsync();
                    if (pendingMigrations.Any())
                    {
                        await db.Database.MigrateAsync();
                    }
                });

                await ScryfallImageDownloader.RunAsync(testMode: false);
                await DbHelper.SeedDb(db);
            }

            app.MapControllers();
            app.MapOpenApi();
            app.MapScalarApiReference();

            app.Run();
        }
    }
}
