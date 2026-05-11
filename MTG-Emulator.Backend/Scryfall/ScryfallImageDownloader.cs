using System.Text.Json;

namespace MTG_Emulator.Backend.Scryfall
{
    public static class ScryfallImageDownloader
    {
        private static readonly string dataRoot = Environment.GetEnvironmentVariable("SCRYFALL_DATA_PATH")
                                                  ?? Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..",
                                                      "scryfall-data");

        private static readonly string bulkJsonPath = Path.Combine(dataRoot, "oracle-cards.json");
        private static readonly string defaultCardsPath = Path.Combine(dataRoot, "default-cards.json");
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

            Console.WriteLine("Reading oracle cards JSON...");
            await using var oracleStream = File.OpenRead(bulkJsonPath);
            var allCards = (await JsonSerializer.DeserializeAsync<List<JsonElement>>(oracleStream))!
                .Where(includeCard)
                .ToList();

            Console.WriteLine($"Found {allCards.Count} paper cards after filtering.");

            // Build token image lookup from default_cards
            Console.WriteLine("Building token image lookup from default_cards...");
            var tokenImageUrls = await buildTokenImageLookupAsync();
            Console.WriteLine($"Found {tokenImageUrls.Count} token image URLs.");

            Console.WriteLine("Starting download...\n");
            await downloadAllAsync(outputFolder, allCards, tokenImageUrls, testMode);
        }

        private static async Task<Dictionary<string, string>> buildTokenImageLookupAsync()
        {
            var lookup = new Dictionary<string, string>();

            await using var stream = File.OpenRead(defaultCardsPath);
            var cards = await JsonSerializer.DeserializeAsync<List<JsonElement>>(stream);

            if (cards == null) return lookup;

            foreach (var card in cards)
            {
                if (!card.TryGetProperty("layout", out var layout) || layout.GetString() != "token")
                    continue;

                if (!card.TryGetProperty("id", out var id))
                    continue;

                // Single faced token
                if (card.TryGetProperty("image_uris", out var imageUris) &&
                    imageUris.TryGetProperty("normal", out var normal))
                {
                    lookup[id.GetString()!] = normal.GetString()!;
                }
                // Double faced token
                else if (card.TryGetProperty("card_faces", out var faces))
                {
                    int faceIndex = 0;
                    foreach (var face in faces.EnumerateArray())
                    {
                        if (face.TryGetProperty("image_uris", out var faceUris) &&
                            faceUris.TryGetProperty("normal", out var faceNormal))
                        {
                            lookup[$"{id.GetString()!}_face{faceIndex}"] = faceNormal.GetString()!;
                        }
                        faceIndex++;
                    }
                }
            }

            return lookup;
        }

        private static async Task downloadAllAsync(
            string outputFolder,
            List<JsonElement> cards,
            Dictionary<string, string> tokenImageUrls,
            bool testMode)
        {
            // Collect token IDs referenced by oracle cards
            var referencedTokenIds = new HashSet<string>();
            foreach (var card in cards)
            {
                if (!card.TryGetProperty("all_parts", out var parts)) continue;
                var gameComponents = new HashSet<string> { "token", "emblem" };
                foreach (var part in parts.EnumerateArray())
                {
                    if (part.TryGetProperty("component", out var comp) &&
                        gameComponents.Contains(comp.GetString()!) &&
                        part.TryGetProperty("id", out var partId))
                    {
                        referencedTokenIds.Add(partId.GetString()!);
                    }
                }
            }

            var cardsToProcess = testMode ? getTestSample(cards) : cards;

            int done = 0, skipped = 0, failed = 0;

            // Download oracle cards
            foreach (var card in cardsToProcess)
            {
                string layout   = card.GetProperty("layout").GetString()!;
                string cardId   = layout == "token"
                    ? card.GetProperty("id").GetString()!
                    : card.GetProperty("oracle_id").GetString()!;
                string cardName = card.GetProperty("name").GetString()!;

                if (card.TryGetProperty("image_uris", out var imageUris))
                {
                    if (!imageUris.TryGetProperty("normal", out var normalProp)) { skipped++; continue; }

                    var result = await downloadCardImageAsync(outputFolder, normalProp.GetString()!, cardId, cardName);
                    trackResult(result, ref done, ref skipped, ref failed);
                    if (result == DownloadResult.Downloaded) await Task.Delay(delayMs);
                }
                else if (card.TryGetProperty("card_faces", out var faces))
                {
                    int faceIndex = 0;
                    foreach (var face in faces.EnumerateArray())
                    {
                        if (!face.TryGetProperty("image_uris", out var faceUris) ||
                            !faceUris.TryGetProperty("normal", out var faceNormal))
                        {
                            faceIndex++; continue;
                        }

                        string faceName = face.GetProperty("name").GetString()!;
                        var result = await downloadCardImageAsync(
                            outputFolder, faceNormal.GetString()!, $"{cardId}_face{faceIndex}", faceName);

                        trackResult(result, ref done, ref skipped, ref failed);
                        if (result == DownloadResult.Downloaded) await Task.Delay(delayMs);
                        faceIndex++;
                    }
                }
                else { skipped++; }

                int total = done + skipped + failed;
                if (total % 100 == 0 && total > 0)
                    Console.WriteLine($"  Progress: {done} downloaded, {skipped} skipped, {failed} failed");
            }

            // Download referenced token images from default_cards lookup
            Console.WriteLine($"\nDownloading {referencedTokenIds.Count} referenced token images...");
            foreach (var tokenId in referencedTokenIds)
            {
                // Check single-faced
                if (tokenImageUrls.TryGetValue(tokenId, out var url))
                {
                    var result = await downloadCardImageAsync(outputFolder, url, tokenId, tokenId);
                    trackResult(result, ref done, ref skipped, ref failed);
                    if (result == DownloadResult.Downloaded) await Task.Delay(delayMs);
                }
                else
                {
                    // Check double-faced (face0, face1)
                    for (int i = 0; i < 2; i++)
                    {
                        string faceKey = $"{tokenId}_face{i}";
                        if (tokenImageUrls.TryGetValue(faceKey, out var faceUrl))
                        {
                            var result = await downloadCardImageAsync(outputFolder, faceUrl, faceKey, faceKey);
                            trackResult(result, ref done, ref skipped, ref failed);
                            if (result == DownloadResult.Downloaded) await Task.Delay(delayMs);
                        }
                    }
                }
            }

            Console.WriteLine($"\nFinished! {done} downloaded, {skipped} skipped, {failed} failed.");
        }

        private static void trackResult(DownloadResult result, ref int done, ref int skipped, ref int failed)
        {
            switch (result)
            {
                case DownloadResult.Downloaded:   done++;    break;
                case DownloadResult.AlreadyExists: skipped++; break;
                case DownloadResult.Failed:        failed++;  break;
            }
        }

        private static List<JsonElement> getTestSample(List<JsonElement> cards)
        {
            var singleFaced = cards.Where(c => c.TryGetProperty("image_uris", out _)).Take(25);
            var doubleFaced = cards.Where(c => c.TryGetProperty("card_faces", out _)).Take(25);
            var sample = singleFaced.Concat(doubleFaced).ToList();
            Console.WriteLine($"Test sample: {sample.Count} cards");
            return sample;
        }

        private static async Task ensureBulkDataExistsAsync()
        {
            var bulkIndex = await fetchBulkIndexAsync();

            await ensureFileDownloadedAsync(bulkJsonPath, "oracle_cards", bulkIndex, "oracle cards");
            await ensureFileDownloadedAsync(defaultCardsPath, "default_cards", bulkIndex, "default cards");
        }

        private static async Task<JsonDocument> fetchBulkIndexAsync()
        {
            Console.WriteLine("Fetching Scryfall bulk data index...");
            var response = await http.GetAsync("https://api.scryfall.com/bulk-data");
            response.EnsureSuccessStatusCode();
            return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        }

        private static async Task ensureFileDownloadedAsync(
            string filePath, string type, JsonDocument bulkIndex, string label)
        {
            if (File.Exists(filePath))
            {
                Console.WriteLine($"{label} already exists, skipping download.");
                return;
            }

            string? downloadUrl = null;
            foreach (var item in bulkIndex.RootElement.GetProperty("data").EnumerateArray())
            {
                if (item.GetProperty("type").GetString() == type)
                {
                    downloadUrl = item.GetProperty("download_uri").GetString();
                    break;
                }
            }

            if (downloadUrl is null)
                throw new Exception($"Could not find {type} in Scryfall bulk data index.");

            Console.WriteLine($"Downloading {label} from {downloadUrl} ...");
            var bytes = await http.GetByteArrayAsync(downloadUrl);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            await File.WriteAllBytesAsync(filePath, bytes);
            Console.WriteLine($"{label} saved.\n");
        }

        private static async Task<DownloadResult> downloadCardImageAsync(
            string outputFolder, string url, string fileId, string cardName)
        {
            string fullPath = Path.Combine(outputFolder, $"{fileId}.jpg");

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
                    if (attempt < maxRetries) await Task.Delay(200 * attempt);
                }
            }

            return DownloadResult.Failed;
        }

        private enum DownloadResult { Downloaded, AlreadyExists, Failed }
    }
}