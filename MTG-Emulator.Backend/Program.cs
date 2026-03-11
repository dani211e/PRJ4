using MTG_Emulator.Backend.DB;
using MTG_Emulator.Backend.DB.Models;

//var builder = WebApplication.CreateBuilder(args);

using (var db = new MTGContext())
{
    seedDb(db);
}

void seedDb(MTGContext db)
{
    var c1 = new Card { Name = "Test", OracleText = "Very good card", ImageURI = "https://cards.scryfall.io/large/front/a/9/a9b2a843-c6fe-4d19-801e-1538e4381ab0.jpg?1764119928"};
    var c2 = new Card { Name = "Good card", OracleText = "Even better card", ImageURI = "https://cards.scryfall.io/large/front/8/2/829d91e9-4878-4e55-a262-ac0d55b65d4e.jpg?1764119935"};

    var player1 = new Player {Username = "Kasper", Password = "1234", GamesWon = 4, GamesLost = 0, GamesDrawed = 2 };

    var deck1 = new Deck { DeckName = "Best deck ever", Cards = new List<Card>{c1,c1,c1,c2,c2,c2,c2}, DeckCommander = c1.Name, Player = player1};

    db.Add(c1);
    db.Add(c2);
    db.Add(player1);
    db.Add(deck1);
    db.SaveChanges();


}
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

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}*/
