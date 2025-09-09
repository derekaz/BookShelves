using BookShelves.Shared.DataInterfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShelves.Maui.Data.Models
{
    public class RemoteBook : IItem, IBook
    {
        public const string BOOKS_UNIQUEID_RECORD_ID = "**UNIQUEID**";

        [JsonProperty(PropertyName = "id")]
        public string? Id { get; set; } = string.Empty;

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
}
