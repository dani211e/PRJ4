using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using MTG_Emulator.Backend;
using MTG_Emulator.Backend.DB;
using MTG_Emulator.Backend.DB.Models;
using Scalar.AspNetCore;

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

        var app = builder.Build();

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
