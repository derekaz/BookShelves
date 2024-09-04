using Newtonsoft.Json;
using BookShelves.Shared.DataInterfaces;

public class Book : IBook
{
    [JsonProperty(PropertyName = "id")]
    public string? Id { get; set; }

    public string IdValue => Id ?? string.Empty;

    [JsonProperty(PropertyName = "title")]
    public string? Title { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "author")]
    public string? Author { get; set; } = string.Empty;
}
