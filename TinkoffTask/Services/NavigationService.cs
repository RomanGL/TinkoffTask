using DryIoc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TinkoffTask.ViewModels;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace TinkoffTask.Services
{
    public class NavigationService : INavigationService
    {
        private readonly Frame _frame;
        private readonly IResolver _resolver;
        private readonly SystemNavigationManager _currentNavigationManager;
        private readonly Dictionary<Type, Type> _registrations = new Dictionary<Type, Type>();

        public NavigationService(Frame frame, IResolver resolver)
        {
            _frame = frame ?? throw new ArgumentNullException(nameof(frame));
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));

            _currentNavigationManager = SystemNavigationManager.GetForCurrentView();
            _currentNavigationManager.BackRequested += CurrentNavigationManager_BackRequested;
            _frame.Navigated += Frame_Navigated;
        }

        public async Task NavigateAsync<TViewModel>(object parameter = null)
        {
            try
            {
                var viewType = _registrations[typeof(TViewModel)];
                bool navigationResult = _frame.Navigate(viewType, parameter);

                if (!navigationResult)
                {
                    throw new NavigationException($"Cannot navigate to {viewType.Name}.");
                }

                var viewModel = _resolver.Resolve<TViewModel>();
                if (_frame.Content is FrameworkElement view)
                {
                    view.DataContext = viewModel;
                }

                if (viewModel is IViewModel vm)
                {
                    await vm.OnNavigatedToAsync(parameter);
                }
            }
            catch (KeyNotFoundException)
            {
                throw new NavigationException($"Registered view for the {typeof(TViewModel).Name} not found.");
            }
        }

        public bool CanGoBack() => _frame.CanGoBack;

        public void GoBack()
        {
            if (CanGoBack())
            {
                _frame.GoBack();
            }
        }

        public void MapViewModelToView<TViewModel, TView>()
        {
            _registrations[typeof(TViewModel)] = typeof(TView);
        }

        private void CurrentNavigationManager_BackRequested(object sender, BackRequestedEventArgs e)
            => GoBack();

        private void Frame_Navigated(object sender, NavigationEventArgs e)
        {
            _currentNavigationManager.AppViewBackButtonVisibility = 
                CanGoBack() ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
        }
    }
}
