using BookShelves.Shared.Data.Interfaces;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace BookShelves.WasmApi.DataAccess;

public class UniqueId : IItem
{
    
    [JsonProperty(PropertyName = "id")]
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonProperty(PropertyName = "uniqueIdValue")]
    [JsonPropertyName("uniqueIdValue")]
    public long UniqueIdValue { get; set; }
}
