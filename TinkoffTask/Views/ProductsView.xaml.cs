using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace TinkoffTask.Views
{
    public sealed partial class ProductsView : Page
    {
        public ProductsView()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Carousel.IsAutoSwitchEnabled = true;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Carousel.IsAutoSwitchEnabled = false;
        }
    }
}
