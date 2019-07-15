using System;
using System.Threading.Tasks;
using TinkoffTask.Common;
using TinkoffTask.Models;
using Windows.System;

namespace TinkoffTask.ViewModels
{
    public sealed class ProductViewModel : ViewModelBase
    {
        public ProductViewModel()
        {
            OpenDetails = new DelegateCommand(OnOpenDetails);
        }

        public DelegateCommand OpenDetails { get; }
        public TinkoffProduct Product { get; private set; }

        public override Task OnNavigatedToAsync(object parameter)
        {
            Product = parameter as TinkoffProduct ?? throw new ArgumentNullException(nameof(parameter));
            return Task.CompletedTask;
        }

        private async void OnOpenDetails()
            => await Launcher.LaunchUriAsync(new Uri("https://tinkoff.ru" + Product.TariffUrl));
    }
}
