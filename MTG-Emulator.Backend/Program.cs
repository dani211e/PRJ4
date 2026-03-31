using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MTG_Emulator.Backend.DB;
using Scalar.AspNetCore;

namespace MTG_Emulator.Backend
{
    internal abstract class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddOpenApi();

            builder.Services.AddDbContext<MTGContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

            app.UseHttpLogging();

            using (var scope = app.Services.CreateScope())
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

                var db = scope.ServiceProvider.GetRequiredService<MTGContext>();
                await DbHelper.SeedDb(db, httpClient);
            }

            app.MapControllers();
            app.MapOpenApi();
            app.MapScalarApiReference();

            app.Run();
        }
    }
}
