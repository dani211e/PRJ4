using System.Text.Json.Serialization;

namespace MTG_Emulator.Backend.Scryfall
{
    public struct ScryfallBulkResponse
    {
        [JsonPropertyName("object")]
        public string Object {get; set;}

        [JsonPropertyName("has_more")]
        public bool HasMore {get; set;}

        [JsonPropertyName("data")]
        public ScryfallBulkDataResponse[] Data {get; set;}
    }

    public struct ScryfallBulkDataResponse
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("uri")]
        public string Uri { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("download_uri")]
        public string DownloadUri { get; set; }

        [JsonPropertyName("updated_at")]
        public string UpdatedAt { get; set; }

        [JsonPropertyName("size")]
        public long Size { get; set; }

        [JsonPropertyName("content_type")]
        public string ContentType { get; set; }

        [JsonPropertyName("content_encoding")]
        public string ContentEncoding { get; set; }
    }
}
