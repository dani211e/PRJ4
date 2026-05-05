using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public static class ScryfallImageDownloader
{
    static readonly string DataRoot = Environment.GetEnvironmentVariable("SCRYFALL_DATA_PATH")
                                      ?? Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "scryfall-data");

    static readonly string BulkJsonPath = Path.Combine(DataRoot, "oracle-cards.json");
    const int DelayMs    = 75;
    const int MaxRetries = 3;

    static readonly HttpClient Http = new();

    private enum DownloadResult { Downloaded, AlreadyExists, Failed }

    static readonly HashSet<string> ExcludedLayouts = new()
    {
        "art_series",
        "reversible_card",
        "planar",
        "scheme",
        "vanguard",
        "conspiracy",
        "double_faced_token"
    };

    static bool IncludeCard(JsonElement card) =>
        card.TryGetProperty("games", out var games) &&
        games.EnumerateArray().Any(g => g.GetString() == "paper") &&
        card.TryGetProperty("layout", out var layout) &&
        !ExcludedLayouts.Contains(layout.GetString()!);

    private static List<JsonElement> GetTestSample(List<JsonElement> cards)
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

    public static async Task RunAsync( bool testMode = false)
    {
        string outputFolder = Path.Combine(DataRoot, "images");

        if (Directory.Exists(outputFolder) && Directory.EnumerateFiles(outputFolder).Any())
        {
            Console.WriteLine("Images already exist, skipping download.");
            return;
        }

        Http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "MyMtgApp/1.0 (you@example.com)");
        Http.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json;q=0.9,*/*;q=0.8");

        await EnsureBulkDataExistsAsync();

        Directory.CreateDirectory(outputFolder);

        Console.WriteLine("Reading bulk JSON...");
        await using var stream = File.OpenRead(BulkJsonPath);

        var allCards = (await JsonSerializer.DeserializeAsync<List<JsonElement>>(stream))!
            .Where(IncludeCard)
            .ToList();

        Console.WriteLine($"Found {allCards.Count} paper cards after filtering.");

        var cardsToProcess = testMode ? GetTestSample(allCards) : allCards;

        Console.WriteLine("Starting download...\n");

        await DownloadAllAsync(outputFolder, cardsToProcess);
    }

    private static async Task DownloadAllAsync(string outputFolder, List<JsonElement> cards)
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

                var result = await DownloadCardImageAsync(outputFolder, normalProp.GetString()!, cardId, cardName);

                if (result == DownloadResult.Failed) failed++;
                else if (result == DownloadResult.AlreadyExists) skipped++;
                else
                {
                    done++;
                    await Task.Delay(DelayMs);
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

                    var result = await DownloadCardImageAsync(
                        outputFolder,
                        faceNormal.GetString()!,
                        $"{cardId}_face{faceIndex}",
                        faceName
                    );

                    if (result == DownloadResult.Failed) failed++;
                    else if (result == DownloadResult.AlreadyExists) skipped++;
                    else
                    {
                        done++;
                        await Task.Delay(DelayMs);
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

    private static async Task EnsureBulkDataExistsAsync()
    {
        if (File.Exists(BulkJsonPath)) return;

        Console.WriteLine("Bulk data file not found, downloading from Scryfall...");

        var response = await Http.GetAsync("https://api.scryfall.com/bulk-data");
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
        var bytes = await Http.GetByteArrayAsync(downloadUrl);

        Directory.CreateDirectory(Path.GetDirectoryName(BulkJsonPath)!);
        await File.WriteAllBytesAsync(BulkJsonPath, bytes);

        Console.WriteLine("Bulk data saved.\n");
    }

    private static async Task<DownloadResult> DownloadCardImageAsync(
        string outputFolder, string url, string fileId, string cardName)
    {
        string fileName = $"{fileId}.jpg";
        string fullPath = Path.Combine(outputFolder, fileName);

        if (File.Exists(fullPath))
            return DownloadResult.AlreadyExists;

        for (int attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                var bytes = await Http.GetByteArrayAsync(url);
                await File.WriteAllBytesAsync(fullPath, bytes);

                Console.WriteLine($"  [OK]   {cardName}");
                return DownloadResult.Downloaded;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  [ERR]  Attempt {attempt}/{MaxRetries} for {cardName}: {ex.Message}");

                if (attempt < MaxRetries)
                    await Task.Delay(200 * attempt);
            }
        }

        return DownloadResult.Failed;
    }
}
