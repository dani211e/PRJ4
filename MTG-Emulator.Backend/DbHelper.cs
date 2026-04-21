using System.Text.Json;
using System.Text.Json.Serialization;
using MTG_Emulator.Backend.DB;
using MTG_Emulator.Backend.DB.Models;
using MTG_Emulator.Backend.Scryfall;

namespace MTG_Emulator.Backend
{
    public static class DbHelper
    {
        private static readonly string bulkDataPath = Path.Combine(
            Environment.GetEnvironmentVariable("SCRYFALL_DATA_PATH")
            ?? Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "scryfall-data"),
            "oracle-cards.json");

        private static readonly HashSet<string> ExcludedLayouts = new()
        {
            "art_series",
            "reversible_card",
            "planar",
            "scheme",
            "vanguard",
        };
            "conspiracy",

        public static async Task SeedDb(MTGContext db, int? count = null, bool forceSeed = false)
        {
            if (!forceSeed && db.Cards.Any())
                return;

            if (!File.Exists(bulkDataPath))
                throw new FileNotFoundException($"Bulk card data not found at: {bulkDataPath}. Ensure the downloader has run first.");

            var cardsEnum = readScryfallCards(bulkDataPath)
                .Where(c => !ExcludedLayouts.Contains(c.Layout));

            if (count.HasValue)
                cardsEnum = cardsEnum.Take(count.Value);

            var cards = await cardsEnum.Select(c => c.ToCard()).ToListAsync();

            var player1 = new Player
            {
                Username = "Kasper",
                Password = "Loser",
                GamesWon = 0,
                GamesLost = 1242,
                GamesDrawn = 2,
            };

            var deck1 = new Deck
            {
                DeckName = "Best deck ever",
                Cards = cards,
                DeckCommander = cards[0].Name,
                Player = player1,
            };

            if (!db.Cards.Any())
                db.AddRange(cards);
            if (!db.Players.Any())
                db.Add(player1);
            if (!db.Decks.Any())
                db.Add(deck1);

            await db.SaveChangesAsync();
        }

        private static async IAsyncEnumerable<ScryfallCard> readScryfallCards(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"File not found at path: {path}");

            await using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            var opt = new JsonSerializerOptions { UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip };

            await foreach (var card in JsonSerializer.DeserializeAsyncEnumerable<ScryfallCard>(fs, opt))
                yield return card;
        }
    }
}
