using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using MTG_Emulator.Backend.DB;
using MTG_Emulator.Backend.DB.Models;
using MTG_Emulator.Backend.Scryfall.ApiResponses;

namespace MTG_Emulator.Backend
{
    public static class DbHelper
    {
        private static readonly string bulkDataPath = Path.Combine(
            Environment.GetEnvironmentVariable("SCRYFALL_DATA_PATH")
            ?? Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "scryfall-data"),
            "oracle-cards.json");

        private static readonly HashSet<string> excludedLayouts =
        [
            "art_series",
            "reversible_card",
            "planar",
            "scheme",
            "vanguard",
            "conspiracy",
        ];

        public static async Task SeedDb(
            MTGContext db,
            UserManager<ApiUser> userManager,
            int? count = null,
            bool forceSeed = false)
        {
            if (!forceSeed && db.Cards.Any())
                return;

            if (!File.Exists(bulkDataPath))
                throw new FileNotFoundException(
                    $"Bulk card data not found at: {bulkDataPath}. Ensure the downloader has run first.");

            var cardsEnum = readScryfallCardsAsync(bulkDataPath)
                .Where(c => !excludedLayouts.Contains(c.Layout));

            if (count.HasValue)
                cardsEnum = cardsEnum.Take(count.Value);

            var cards = await cardsEnum
                .Select(c => c.ToCard())
                .ToListAsync();

            var existingUser = await userManager.FindByEmailAsync("kasper@test.com");

            ApiUser user;

            if (existingUser == null)
            {
                user = new ApiUser
                {
                    UserName = "Kasper",
                    Email = "kasper@test.com"
                };

                var createResult = await userManager.CreateAsync(user, "Password123!");

                if (!createResult.Succeeded)
                {
                    throw new Exception(
                        string.Join(", ", createResult.Errors.Select(e => e.Description)));
                }

                await userManager.AddToRoleAsync(user, Roles.Player);
            }
            else
            {
                user = existingUser;
            }

            var existingPlayer = db.Players
                .FirstOrDefault(p => p.ApiUserId == user.Id);

            Player player1;

            if (existingPlayer == null)
            {
                player1 = new Player
                {
                    Username = "Kasper",
                    GamesWon = 0,
                    GamesLost = 1242,
                    GamesDrawn = 2,
                    ApiUserId = user.Id
                };

                db.Players.Add(player1);
                await db.SaveChangesAsync();
            }
            else
            {
                player1 = existingPlayer;
            }

            if (!db.Cards.Any())
            {
                db.Cards.AddRange(cards);
                await db.SaveChangesAsync();
            }

            if (!db.Decks.Any())
            {
                var deck1 = new Deck
                {
                    DeckName = "Best deck ever",
                    CommanderName = cards.FirstOrDefault()?.Name ?? "Unknown Commander",
                    Player = player1,
                    DeckCards = cards.Select(c => new DeckCard
                    {
                        Card = c,
                        Quantity = 1
                    }).ToList(),
                };

                db.Decks.Add(deck1);
                await db.SaveChangesAsync();
            }
        }

        private static async IAsyncEnumerable<ScryfallCard> readScryfallCardsAsync(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"File not found at path: {path}");

            await using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);

            var opt = new JsonSerializerOptions
            {
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip
            };

            await foreach (var card in JsonSerializer.DeserializeAsyncEnumerable<ScryfallCard>(fs, opt))
            {
                yield return card;
            }
        }
    }
}