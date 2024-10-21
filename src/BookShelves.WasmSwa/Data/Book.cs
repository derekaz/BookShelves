using Newtonsoft.Json;
using BookShelves.Shared.DataInterfaces;
using Newtonsoft.Json.Serialization;

public class Book : IBook
{
    [JsonProperty(PropertyName = "id")]
    public string? Id { get; set; }

    public string IdValue => Id ?? string.Empty;

    [JsonProperty(PropertyName = "title")]
    public string? Title { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "author")]
    public string? Author { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "lastUpdateTime")]
    public DateTime? LastUpdateTime { get; set; } = DateTime.UtcNow;

    [JsonProperty(PropertyName = "revision")]
    public int? Revision { get; set; } = 0;
}
