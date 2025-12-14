using System;

using EE_Calculator.ViewModels;

using Windows.UI.Xaml.Controls;

namespace EE_Calculator.Views
{
    public sealed partial class HelpPage : Page
    {
        public HelpViewModel ViewModel { get; } = new HelpViewModel();

        public HelpPage()
        {
            InitializeComponent();
        }
    }
}
