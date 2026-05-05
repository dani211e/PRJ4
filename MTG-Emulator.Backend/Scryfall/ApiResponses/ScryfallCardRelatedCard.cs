using System.Text.Json.Serialization;

namespace MTG_Emulator.Backend.Scryfall
{
    public struct ScryfallCardRelatedCard
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        //[JsonPropertyName("object")]
        //public string CardObject { get; set; }

        [JsonPropertyName("component")]
        public string Component { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type_line")]
        public string TypeLine { get; set; }

        [JsonPropertyName("uri")]
        public string Uri { get; set; }
    }
}
