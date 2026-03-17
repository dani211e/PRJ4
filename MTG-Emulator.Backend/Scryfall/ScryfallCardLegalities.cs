using System.Text.Json.Serialization;

namespace MTG_Emulator.Backend.Scryfall
{
    public struct ScryfallCardLegalities
    {
        [JsonPropertyName("standard")] public string Standard { get; set; }

        [JsonPropertyName("future")] public string Future { get; set; }

        [JsonPropertyName("historic")] public string Historic { get; set; }

        [JsonPropertyName("timeless")] public string Timeless { get; set; }

        [JsonPropertyName("gladiator")] public string Gladiator { get; set; }

        [JsonPropertyName("pioneer")] public string Pioneer { get; set; }

        [JsonPropertyName("modern")] public string Modern { get; set; }

        [JsonPropertyName("legacy")] public string Legacy { get; set; }

        [JsonPropertyName("pauper")] public string Pauper { get; set; }

        [JsonPropertyName("vintage")] public string Vintage { get; set; }

        [JsonPropertyName("penny")] public string Penny { get; set; }

        [JsonPropertyName("commander")] public string Commander { get; set; }

        [JsonPropertyName("oathbreaker")] public string Oathbreaker { get; set; }

        [JsonPropertyName("standardbrawl")] public string Standardbrawl { get; set; }

        [JsonPropertyName("brawl")] public string Brawl { get; set; }

        [JsonPropertyName("alchemy")] public string Alchemy { get; set; }

        [JsonPropertyName("paupercommander")] public string Paupercommander { get; set; }

        [JsonPropertyName("duel")] public string Duel { get; set; }

        [JsonPropertyName("oldschool")] public string Oldschool { get; set; }

        [JsonPropertyName("premodern")] public string Premodern { get; set; }

        [JsonPropertyName("predh")] public string Predh { get; set; }
    }
}
