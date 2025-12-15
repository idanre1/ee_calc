using EE_Calculator.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace EE_Calculator.Views
{
    public sealed partial class CalculatorPage : Page
    {
        public MainViewModel ViewModel { get; } = new MainViewModel();

        public CalculatorPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
        }
    }
}
