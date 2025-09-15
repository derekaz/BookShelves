using BookShelves.Shared.DataInterfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BookShelves.WasmApi.DataAccess
{
    public class UniqueId : IItem
    {
        
        [JsonProperty(PropertyName = "id")]
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonProperty(PropertyName = "uniqueIdValue")]
        [JsonPropertyName("uniqueIdValue")]
        public long UniqueIdValue { get; set; }
    }
}
