using Newtonsoft.Json;
using System.Collections.Generic;

namespace TinkoffTask.Models
{
    public sealed class TinkoffProduct
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("programId")]
        public string ProgramId { get; set; }

        [JsonProperty("product")]
        public string Product { get; set; }

        [JsonProperty("order")]
        public int Order { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("slogan")]
        public string Slogan { get; set; }

        [JsonProperty("multiCurrencies")]
        public bool MultiCurrencies { get; set; }

        [JsonProperty("benefits")]
        public List<ProductBenefit> Benefits { get; set; }

        [JsonProperty("hrefTariff")]
        public string TariffUrl { get; set; }

        [JsonProperty("bgColor")]
        public string BackgroundColor { get; set; }

        [JsonIgnore]
        public string ImageUrl => $"https://static.tcsbank.ru/icons/new-products/windows/{Type}/400/{ProgramId}.png";
    }
}
