using DryIoc;
using System.Threading.Tasks;
using TinkoffTask.Services;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace TinkoffTask
{
    public abstract class ApplicationBase : Application
    {
        protected ApplicationBase()
        {
            Container = new Container();
        }

        protected Container Container { get; private set; }
        protected INavigationService NavigationService { get; private set; }
                
        protected abstract Task OnLaunchApplicationAsync(IActivatedEventArgs args);
        protected virtual Task OnInitializeAsync(IActivatedEventArgs args) => Task.CompletedTask;
        protected virtual void ConfigureNavigationService(INavigationService navigationService) { }
        protected virtual UIElement CreateShell(Frame frame) => frame;
        protected virtual void ConfigureWindow() { }        

        protected override async void OnLaunched(LaunchActivatedEventArgs args) => await InitializeApplicationAsync(args);
        protected override async void OnActivated(IActivatedEventArgs args) => await InitializeApplicationAsync(args);

        private async Task InitializeApplicationAsync(IActivatedEventArgs args)
        {
            if (Window.Current.Content == null)
            {
                var frame = new Frame();
                if (NavigationService == null)
                {
                    NavigationService = new NavigationService(frame, Container);
                    ConfigureNavigationService(NavigationService);
                    Container.RegisterInstance<INavigationService>(NavigationService);
                }

                await OnInitializeAsync(args);

                Window.Current.Content = CreateShell(frame);
                ConfigureWindow();
            }

            await OnLaunchApplicationAsync(args);
            Window.Current.Activate();
        }
    }
}
