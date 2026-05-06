using System.Text.Json;

namespace MTG_Emulator.Backend.Scryfall
{
    public static class ScryfallImageDownloader
    {
        private static readonly string dataRoot = Environment.GetEnvironmentVariable("SCRYFALL_DATA_PATH")
                                                  ?? Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..",
                                                      "scryfall-data");

        private static readonly string bulkJsonPath = Path.Combine(dataRoot, "oracle-cards.json");
        private const int delayMs = 75;
        private const int maxRetries = 3;

        private static readonly HttpClient http = new HttpClient();

        private static readonly HashSet<string> excludedLayouts =
        [
            "art_series",
            "reversible_card",
            "planar",
            "scheme",
            "vanguard",
            "conspiracy",
            "double_faced_token",
        ];

        private static bool includeCard(JsonElement card) =>
            card.TryGetProperty("games", out var games) &&
            games.EnumerateArray().Any(g => g.GetString() == "paper") &&
            card.TryGetProperty("layout", out var layout) &&
            !excludedLayouts.Contains(layout.GetString()!);

        private static List<JsonElement> getTestSample(List<JsonElement> cards)
        {
            var singleFaced = cards
                .Where(c => c.TryGetProperty("image_uris", out _))
                .Take(25);

            var doubleFaced = cards
                .Where(c => c.TryGetProperty("card_faces", out _))
                .Take(25);

            var sample = singleFaced.Concat(doubleFaced).ToList();
            Console.WriteLine($"Test sample: {sample.Count} cards (25 single-faced + 25 double-faced)");
            return sample;
        }

        public static async Task RunAsync(bool testMode = false)
        {
            string outputFolder = Path.Combine(dataRoot, "images");

            if (Directory.Exists(outputFolder) && Directory.EnumerateFiles(outputFolder).Any())
            {
                Console.WriteLine("Images already exist, skipping download.");
                return;
            }

            http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "MyMtgApp/1.0 (you@example.com)");
            http.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json;q=0.9,*/*;q=0.8");

            await ensureBulkDataExistsAsync();

            Directory.CreateDirectory(outputFolder);

            Console.WriteLine("Reading bulk JSON...");
            await using var stream = File.OpenRead(bulkJsonPath);

            var allCards = (await JsonSerializer.DeserializeAsync<List<JsonElement>>(stream))!
                .Where(includeCard)
                .ToList();

            Console.WriteLine($"Found {allCards.Count} paper cards after filtering.");

            var cardsToProcess = testMode ? getTestSample(allCards) : allCards;

            Console.WriteLine("Starting download...\n");

            await downloadAllAsync(outputFolder, cardsToProcess);
        }

        private static async Task downloadAllAsync(string outputFolder, List<JsonElement> cards)
        {
            int done = 0, skipped = 0, failed = 0;

            foreach (var card in cards)
            {
                string cardId   = card.GetProperty("oracle_id").GetString()!;
                string cardName = card.GetProperty("name").GetString()!;

                if (card.TryGetProperty("image_uris", out var imageUris))
                {
                    if (!imageUris.TryGetProperty("normal", out var normalProp))
                    {
                        skipped++;
                        continue;
                    }

                    var result = await downloadCardImageAsync(outputFolder, normalProp.GetString()!, cardId, cardName);

                    switch (result)
                    {
                        case DownloadResult.Failed:
                            failed++;
                            break;
                        case DownloadResult.AlreadyExists:
                            skipped++;
                            break;
                        case DownloadResult.Downloaded:
                            done++;
                            await Task.Delay(delayMs);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else if (card.TryGetProperty("card_faces", out var faces))
                {
                    int faceIndex = 0;

                    foreach (var face in faces.EnumerateArray())
                    {
                        if (!face.TryGetProperty("image_uris", out var faceUris) ||
                            !faceUris.TryGetProperty("normal", out var faceNormal))
                        {
                            faceIndex++;
                            continue;
                        }

                        string faceName = face.GetProperty("name").GetString()!;

                        var result = await downloadCardImageAsync(
                            outputFolder,
                            faceNormal.GetString()!,
                            $"{cardId}_face{faceIndex}",
                            faceName
                        );

                        if (result == DownloadResult.Failed)
                            failed++;
                        else if (result == DownloadResult.AlreadyExists)
                            skipped++;
                        else
                        {
                            done++;
                            await Task.Delay(delayMs);
                        }

                        faceIndex++;
                    }
                }
                else
                {
                    skipped++;
                }

                int total = done + skipped + failed;
                if (total % 100 == 0 && total > 0)
                    Console.WriteLine($"  Progress: {done} downloaded, {skipped} skipped, {failed} failed");
            }

            Console.WriteLine($"\nFinished! {done} downloaded, {skipped} skipped, {failed} failed.");
        }

        private static async Task ensureBulkDataExistsAsync()
        {
            if (File.Exists(bulkJsonPath))
                return;

            Console.WriteLine("Bulk data file not found, downloading from Scryfall...");

            var response = await http.GetAsync("https://api.scryfall.com/bulk-data");
            response.EnsureSuccessStatusCode();

            string body = await response.Content.ReadAsStringAsync();
            using var indexDoc = JsonDocument.Parse(body);

            string? downloadUrl = null;
            foreach (var item in indexDoc.RootElement.GetProperty("data").EnumerateArray())
            {
                if (item.GetProperty("type").GetString() == "oracle_cards")
                {
                    downloadUrl = item.GetProperty("download_uri").GetString();
                    break;
                }
            }

            if (downloadUrl is null)
                throw new Exception("Could not find oracle_cards in Scryfall bulk data index.");

            Console.WriteLine($"Downloading from {downloadUrl} ...");
            var bytes = await http.GetByteArrayAsync(downloadUrl);

            Directory.CreateDirectory(Path.GetDirectoryName(bulkJsonPath)!);
            await File.WriteAllBytesAsync(bulkJsonPath, bytes);

            Console.WriteLine("Bulk data saved.\n");
        }

        private static async Task<DownloadResult> downloadCardImageAsync(
            string outputFolder, string url, string fileId, string cardName)
        {
            string fileName = $"{fileId}.jpg";
            string fullPath = Path.Combine(outputFolder, fileName);

            if (File.Exists(fullPath))
                return DownloadResult.AlreadyExists;

            for (var attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    var bytes = await http.GetByteArrayAsync(url);
                    await File.WriteAllBytesAsync(fullPath, bytes);

                    Console.WriteLine($"  [OK]   {cardName}");
                    return DownloadResult.Downloaded;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  [ERR]  Attempt {attempt}/{maxRetries} for {cardName}: {ex.Message}");

                    if (attempt < maxRetries)
                        await Task.Delay(200 * attempt);
                }
            }

            return DownloadResult.Failed;
        }

        private enum DownloadResult
        {
            Downloaded,
            AlreadyExists,
            Failed
        }
    }
}