using System.Net.Http.Headers;
using MTG_Emulator.Backend.DB;

namespace MTG_Emulator.Backend
{
    internal abstract class Program
    {
        public static async Task Main(string[] args)
        {
            //var builder = WebApplication.CreateBuilder(args);

            using HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

            await using var db = new MTGContext();
            await DbHelper.SeedDb(db, httpClient);

            /*
            // Add services to the container.
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            /*builder.Services.AddOpenApi();



            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment()) app.MapOpenApi();

            app.UseHttpsRedirection();

            string[] summaries = new[]
            {
                "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
            };

            app.MapGet("/weatherforecast", () =>
                {
                    var forecast = Enumerable.Range(1, 5).Select(index =>
                            new WeatherForecast
                            (
                                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                                Random.Shared.Next(-20, 55),
                                summaries[Random.Shared.Next(summaries.Length)]
                            ))
                        .ToArray();
                    return forecast;
                })
                .WithName("GetWeatherForecast");

            app.Run();
            */
        }
    }
}
