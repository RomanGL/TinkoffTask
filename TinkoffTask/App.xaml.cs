using DryIoc;
using System.Threading.Tasks;
using TinkoffTask.Services;
using TinkoffTask.ViewModels;
using TinkoffTask.Views;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace TinkoffTask
{
    public sealed partial class App : ApplicationBase
    {
        public App()
        {
            this.InitializeComponent();
        }

        protected override Task OnLaunchApplicationAsync(IActivatedEventArgs args)
        {
            var navigateTask = NavigationService.NavigateAsync<ProductsViewModel>();
            return Task.CompletedTask;
        }

        protected override Task OnInitializeAsync(IActivatedEventArgs args)
        {
            Container.Register<IApiService, ApiService>(Reuse.Singleton);
            Container.Register<ProductsViewModel>();
            Container.Register<ProductViewModel>();

            return base.OnInitializeAsync(args);
        }

        protected override void ConfigureNavigationService(INavigationService navigationService)
        {
            navigationService.MapViewModelToView<ProductsViewModel, ProductsView>();
            navigationService.MapViewModelToView<ProductViewModel, ProductView>();
        }

        protected override void ConfigureWindow()
        {
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;

            var view = ApplicationView.GetForCurrentView();
            view.TitleBar.ButtonBackgroundColor = Colors.Transparent;
            view.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        }
    }
}
