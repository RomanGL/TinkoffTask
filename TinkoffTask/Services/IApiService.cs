using System.Collections.Generic;
using System.Threading.Tasks;
using TinkoffTask.Models;

namespace TinkoffTask.Services
{
    public interface IApiService
    {
        Task<IReadOnlyList<TinkoffProduct>> GetProductsAsync();
    }
}
