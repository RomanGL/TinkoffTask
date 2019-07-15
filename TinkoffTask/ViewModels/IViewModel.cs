using System.Threading.Tasks;

namespace TinkoffTask.ViewModels
{
    public interface IViewModel
    {
        Task OnNavigatedToAsync(object parameter);
    }
}
