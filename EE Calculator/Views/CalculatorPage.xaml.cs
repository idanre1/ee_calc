using EE_Calculator.ViewModels;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace EE_Calculator.Views
{
    public sealed partial class CalculatorPage : Page
    {
        // Static dictionary to store ViewModels per page ID
        private static Dictionary<Guid, MainViewModel> _viewModelCache = new Dictionary<Guid, MainViewModel>();
        
        public MainViewModel ViewModel { get; private set; }
        private Guid _pageId;

        public CalculatorPage()
        {
            InitializeComponent();
            // Disable cache mode - we'll manage instances manually
            NavigationCacheMode = NavigationCacheMode.Disabled;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            // Get the page ID from navigation parameter
            if (e.Parameter is Guid pageId)
            {
                _pageId = pageId;
                
                // Try to get existing ViewModel for this page ID, or create a new one
                if (!_viewModelCache.TryGetValue(pageId, out var viewModel))
                {
                    viewModel = new MainViewModel();
                    _viewModelCache[pageId] = viewModel;
                }
                
                ViewModel = viewModel;
                
                // Update the binding
                Bindings.Update();
            }
        }
    }
}


