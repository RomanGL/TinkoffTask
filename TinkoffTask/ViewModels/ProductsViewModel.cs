using PropertyChanged;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TinkoffTask.Common;
using TinkoffTask.Models;
using TinkoffTask.Services;

namespace TinkoffTask.ViewModels
{
    public sealed class ProductsViewModel : ViewModelBase
    {
        private readonly IApiService _apiService;
        private readonly INavigationService _navigationService;

        public ProductsViewModel(
            IApiService apiService,
            INavigationService navigationService)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            NavigateToProduct = new DelegateCommand<TinkoffProduct>(OnNavigateToProduct);
            ReloadProducts = new DelegateCommand(async () => await LoadProductsAsync());
        }

        [DoNotNotify]
        public DelegateCommand<TinkoffProduct> NavigateToProduct { get; }
        [DoNotNotify]
        public DelegateCommand ReloadProducts { get; }

        public ObservableCollection<TinkoffProduct> Products { get; set; }
        public ContentState ProductsState { get; private set; }

        [AlsoNotifyFor(nameof(SelectedProduct))]
        public int SelectedIndex { get; set; }
        public TinkoffProduct SelectedProduct => Products?[SelectedIndex];

        public override async Task OnNavigatedToAsync(object parameter) => await LoadProductsAsync();

        private async Task LoadProductsAsync()
        {
            try
            {
                ProductsState = ContentState.Loading;

                var products = await _apiService.GetProductsAsync();
                if (!products.Any())
                {
                    ProductsState = ContentState.NoData;
                    return;
                }

                Products = new ObservableCollection<TinkoffProduct>(products);

                ProductsState = ContentState.Normal;
            }
            catch (ApiException)
            {
                ProductsState = ContentState.Error;
            }
        }

        private async void OnNavigateToProduct(TinkoffProduct product)
            => await _navigationService.NavigateAsync<ProductViewModel>(product);
    }
}
