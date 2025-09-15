using BookShelves.Shared.DataInterfaces;
using System.Text.Json.Serialization;

namespace BookShelves.Maui.Data.Models
{
    // [JsonObject(Title ="Book")]
    public class RemoteBook : IItem, IBook
    {
        public const string BOOKS_UNIQUEID_RECORD_ID = "**UNIQUEID**";

        [JsonPropertyName("id")]
        public string? Id { get; set; } = string.Empty;

        public string IdValue => Id ?? string.Empty;

        [JsonPropertyName("title")]
        public string? Title { get; set; } = string.Empty;

        [JsonPropertyName("author")]
        public string? Author { get; set; } = string.Empty;

        [JsonPropertyName("lastUpdateTime")]
        public DateTime? LastUpdateTime { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("revision")]
        public int? Revision { get; set; } = 0;
    }
}
