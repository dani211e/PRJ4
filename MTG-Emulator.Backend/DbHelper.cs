using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;
using MTG_Emulator.Backend.DB;
using MTG_Emulator.Backend.DB.Models;
using MTG_Emulator.Backend.Scryfall;

namespace MTG_Emulator.Backend
{
    public static class DbHelper
    {
        private const string download_file_name = "ScryfallBulkCards.json";
        private static readonly string downloadPath = Path.Combine(Path.GetTempPath(), download_file_name);

        public static async Task SeedDb(MTGContext db, HttpClient client, int? count = null, bool forceSeed = false)
        {
            //Only reseed if file is stale
            if (!forceSeed &&
                File.Exists(downloadPath) && File.GetCreationTime(downloadPath).Date == DateTime.Today.Date)
                return;

            if (downloadPath.IsNullOrEmpty())
                throw new FileNotFoundException($@"File not found at: {downloadPath}");

            await downloadBulkCardsAsync(client);

            var cardsEnum = readScryfallCards(downloadPath!);

            if (count.HasValue)
                cardsEnum = cardsEnum.Take(count.Value);

            var cards = await cardsEnum.Select(c => c.ToCard()).ToListAsync();

            var player1 = new Player
            {
                Username = "Kasper",
                Password = "Loser",
                GamesWon = 0,
                GamesLost = 1242,
                GamesDrawed = 2
            };

            var deck1 = new Deck
            {
                DeckName = "Best deck ever",
                Cards = cards,
                DeckCommander = cards[0].Name,
                Player = player1
            };

            if (!db.Cards.Any())
                db.AddRange(cards);
            if (!db.Players.Any())
                db.Add(player1);
            if (!db.Decks.Any())
                db.Add(deck1);

            await db.SaveChangesAsync();
        }

        private static async Task downloadBulkCardsAsync(HttpClient client)
        {
            string baseUrl = @"https://api.scryfall.com/bulk-data";
            // We need to make an initial request that tells us where the actual download url is located
            var s1 = await client.GetStreamAsync(baseUrl);
            var r = await JsonSerializer.DeserializeAsync<ScryfallBulkResponse>(s1);

            string? downloadUrl = r.Data.First(x => x.Type == "oracle_cards").DownloadUri.ToString();
            if (downloadUrl.IsNullOrEmpty())
                return;

            var s2 = await client.GetStreamAsync(downloadUrl);
            using var fs = new FileStream(downloadPath, FileMode.Create, FileAccess.Write);
            await s2.CopyToAsync(fs);
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
