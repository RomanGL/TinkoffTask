using Newtonsoft.Json;
using System.Collections.Generic;

namespace TinkoffTask.Models
{
    public sealed class ProductsResult
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("value")]
        public List<TinkoffProduct> Value { get; set; }
    }
}
