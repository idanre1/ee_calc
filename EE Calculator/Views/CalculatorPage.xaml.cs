using EE_Calculator.ViewModels;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml;

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
                ViewModel.LastTabClosed += ViewModel_LastTabClosed;
                
                // Update the binding
                Bindings.Update();
            }
        }

        private void ViewModel_LastTabClosed(object sender, EventArgs e)
        {
            // Unsubscribe from the event to avoid multiple calls
            ViewModel.LastTabClosed -= ViewModel_LastTabClosed;

            if (_viewModelCache.ContainsKey(_pageId))
            {
                _viewModelCache.Remove(_pageId);
            }

            var shellPage = Window.Current.Content as ShellPage;
            shellPage?.ViewModel.ClosePage(_pageId);
        }

        private void PageTitleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var shellPage = Window.Current.Content as ShellPage;
            if (shellPage != null)
            {
                shellPage.ViewModel.RenamePage(_pageId, PageTitleTextBox.Text);
            }
        }
    }
}
