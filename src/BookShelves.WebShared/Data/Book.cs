using Newtonsoft.Json;
using BookShelves.Shared.DataInterfaces;

public class Book : IItem, IBook
{
    [JsonProperty(PropertyName = "id")]
    public string? Id { get; set; } = string.Empty;

    public string IdValue => Id ?? string.Empty;

    [JsonProperty(PropertyName = "title")]
    public string? Title { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "author")]
    public string? Author { get; set; } = string.Empty;
}
