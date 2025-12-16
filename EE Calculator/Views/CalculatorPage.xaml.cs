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

        internal static MainViewModel TryGetViewModel(Guid pageId)
        {
            _viewModelCache.TryGetValue(pageId, out var vm);
            return vm;
        }

        internal static MainViewModel GetOrCreateViewModel(Guid pageId)
        {
            if (!_viewModelCache.TryGetValue(pageId, out var vm))
            {
                vm = new MainViewModel();
                _viewModelCache[pageId] = vm;
            }
            return vm;
        }

        public static MainViewModel GetCachedViewModel(Guid pageId)
        {
            _viewModelCache.TryGetValue(pageId, out var viewModel);
            return viewModel;
        }

        public static void RemoveFromCache(Guid pageId)
        {
            if (_viewModelCache.ContainsKey(pageId))
            {
                _viewModelCache.Remove(pageId);
                System.Diagnostics.Debug.WriteLine($"CalculatorPage.RemoveFromCache: Removed page {pageId} from cache");
            }
        }
        
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
            
            System.Diagnostics.Debug.WriteLine($"CalculatorPage.OnNavigatedTo: Called");
            
            // Get the page ID from navigation parameter
            if (e.Parameter is Guid pageId)
            {
                _pageId = pageId;
                System.Diagnostics.Debug.WriteLine($"CalculatorPage.OnNavigatedTo: Page ID = {pageId}");
                
                // Try to get existing ViewModel for this page ID, or create a new one
                bool isNewViewModel = false;
                if (!_viewModelCache.TryGetValue(pageId, out var viewModel))
                {
                    System.Diagnostics.Debug.WriteLine($"CalculatorPage.OnNavigatedTo: Creating new ViewModel for page {pageId}");
                    viewModel = new MainViewModel();
                    viewModel.IsMainPage = false; // This is NOT the main page
                    _viewModelCache[pageId] = viewModel;
                    isNewViewModel = true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"CalculatorPage.OnNavigatedTo: Using cached ViewModel for page {pageId}");
                }
                
                ViewModel = viewModel;
                DataContext = ViewModel;
                ViewModel.LastTabClosed += ViewModel_LastTabClosed;
                
                // Recreate UI controls for all tabs to avoid visual tree conflicts
                ViewModel.RecreateTabControls();
                
                // Update the binding
                Bindings.Update();

                // If this is a new ViewModel and it has no tabs, create the first tab automatically
                if (isNewViewModel && ViewModel.Tabs.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine($"CalculatorPage.OnNavigatedTo: Initializing with default tab");
                    ViewModel.InitializeWithDefaultTab();
                }

                // Initialize page title TextBox from ShellViewModel
                var shellPage = Window.Current.Content as ShellPage;
                var title = shellPage?.ViewModel.GetPageTitle(_pageId);
                if (!string.IsNullOrEmpty(title))
                {
                    PageTitleTextBox.Text = title;
                }

                System.Diagnostics.Debug.WriteLine($"CalculatorPage.OnNavigatedTo: ViewModel tabs count = {ViewModel.Tabs.Count}");
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

        private void TabView_TabCloseRequested(Microsoft.UI.Xaml.Controls.TabView sender, Microsoft.UI.Xaml.Controls.TabViewTabCloseRequestedEventArgs args)
        {
            ViewModel?.CloseTabCommand?.Execute(args);
        }
    }
}
