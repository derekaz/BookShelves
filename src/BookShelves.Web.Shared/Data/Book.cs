using BookShelves.Shared.Data.Interfaces;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace BookShelves.WebShared.Data;

// [JsonObject(Title = "Book")]
public class Book : IItem, IBook
{
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public const string BOOKS_UNIQUEID_RECORD_ID = "**UNIQUEID**";

    [JsonProperty("id")]
    [JsonPropertyName("id")]
    public string? Id { get; set; } = string.Empty;

    public string IdValue => Id ?? string.Empty;

    [JsonProperty("title")]
    [JsonPropertyName("title")]
    public string? Title { get; set; } = string.Empty;

    [JsonProperty("author")]
    [JsonPropertyName("author")]
    public string? Author { get; set; } = string.Empty;

    [JsonProperty("lastUpdateTime")]
    [JsonPropertyName("lastUpdateTime")]
    public DateTime? LastUpdateTime { get; set; } = DateTime.UtcNow;

    [JsonProperty("revision")]
    [JsonPropertyName("revision")]
    public int? Revision { get; set; } = 0;
}
