using Newtonsoft.Json;

namespace TinkoffTask.Models
{
    public sealed class ProductBenefitIcon
    {
        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
