using Newtonsoft.Json;

namespace TinkoffTask.Models
{
    public sealed class ProductBenefit
    {
        [JsonProperty("icon")]
        public ProductBenefitIcon Icon { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
