using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TinkoffTask.Models;
using Windows.Web.Http;

namespace TinkoffTask.Services
{
    public sealed class ApiService : IApiService
    {
        private const string ApiProducts = "https://config.tinkoff.ru/resources?name=products_info";

        public async Task<IReadOnlyList<TinkoffProduct>> GetProductsAsync()
        {
            try
            {
                var response = await GetResponseAsync(ApiProducts);
                var result = JsonConvert.DeserializeObject<ApiResult<ProductsResult>>(response);
                return result.Result.Value.AsReadOnly();
            }
            catch (Exception ex)
            {
                throw new ApiException("Cannot get products. See the inner exception for details.", ex);
            }
        }

        private static async Task<string> GetResponseAsync(string url)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(new Uri(url));
                string responseString = await response.Content.ReadAsStringAsync();

                return responseString;
            }
        }
    }
}
