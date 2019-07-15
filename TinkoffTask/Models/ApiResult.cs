using Newtonsoft.Json;

namespace TinkoffTask.Models
{
    public sealed class ApiResult<T>
    {
        [JsonProperty("result")]
        public T Result { get; set; }
    }
}
