using System;

using EE_Calculator.ViewModels;

using Windows.UI.Xaml.Controls;

namespace EE_Calculator.Views
{
    // For more info about the TabView Control see
    // https://docs.microsoft.com/uwp/api/microsoft.ui.xaml.controls.tabview?view=winui-2.2
    // For other samples, get the XAML Controls Gallery app http://aka.ms/XamlControlsGallery
    public sealed partial class SavedCalcsPage : Page
    {
        public SavedCalcsViewModel ViewModel { get; } = new SavedCalcsViewModel();

        public SavedCalcsPage()
        {
            InitializeComponent();
        }
    }
}
