using System;
using System.Threading.Tasks;

namespace TinkoffTask.Services
{
    public interface INavigationService
    {
        Task NavigateAsync<TViewModel>(object parameter = null);        
        bool CanGoBack();
        void GoBack();
        void MapViewModelToView<TViewModel, TView>();
    }
}
