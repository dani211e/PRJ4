using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
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


            // 1️⃣ Create some players
            var player1 = new Player { Username = "Alice", Password = "Hej" };
            var player2 = new Player { Username = "Bob", Password = "Hej" };

            db.Players.AddRange(player1, player2);
            await db.SaveChangesAsync();

            // 2️⃣ Optional: get some cards from DB
            var cards = await db.Cards.Take(5).ToListAsync();
            if (!cards.Any()) Console.WriteLine("No cards in DB to seed decks.");

            // 3️⃣ Create some decks
            var deck1 = new Deck
            {
                DeckName = "Alice's Aggro Deck",
                DeckCommander = "Goblin King",
                Player = player1,
                Cards = new List<Card>(cards)
            };

            var deck2 = new Deck
            {
                DeckName = "Bob's Control Deck",
                DeckCommander = "Blue Wizard",
                Player = player2,
                Cards = new List<Card>(cards)
            };

            db.Decks.AddRange(deck1, deck2);
            await db.SaveChangesAsync();

            Console.WriteLine("Seeded test players and decks.");

            // await DbHelper.SeedDb(db, httpClient);
        }

        app.MapControllers();
        app.MapOpenApi();
        app.MapScalarApiReference();

        app.Run();
    }
}
