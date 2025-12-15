using EE_Calculator.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace EE_Calculator.Views
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; } = new MainViewModel();

        public MainPage()
        {
            InitializeComponent();
            // Enable navigation cache so the page instance is preserved when navigating away
            NavigationCacheMode = NavigationCacheMode.Enabled;
        }
    }
}


