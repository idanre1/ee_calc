using System;
using System.Collections.Specialized;
using EE_Calculator.ViewModels;
using Windows.UI.Xaml.Controls;
using WinUI = Microsoft.UI.Xaml.Controls;

namespace EE_Calculator.Views
{
    // TODO: Change the icons and titles for all NavigationViewItems in ShellPage.xaml.
    public sealed partial class ShellPage : Page
    {
        public ShellViewModel ViewModel { get; } = new ShellViewModel();
        private bool _isProcessingCollectionChange = false;

        public ShellPage()
        {
            InitializeComponent();
            DataContext = ViewModel;
            ViewModel.Initialize(shellFrame, navigationView, KeyboardAccelerators);

            // Subscribe to dynamic pages collection changes
            ViewModel.DynamicPages.CollectionChanged += DynamicPages_CollectionChanged;

            // Load session asynchronously
            _ = InitializeAsync();

            // Save session when app is suspending
            Windows.UI.Xaml.Application.Current.Suspending += OnAppSuspending;
            Windows.UI.Xaml.Application.Current.EnteredBackground += OnAppEnteredBackground;

            // Navigate to the default page (MainPage) on shell creation
            // so that navigationView.Selection and shellFrame content are in sync
            Services.NavigationService.Navigate(typeof(MainPage));
        }

        private async System.Threading.Tasks.Task InitializeAsync()
        {
            await ViewModel.InitializeAsync();
        }

        private async void OnAppSuspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            try
            {
                await ViewModel.SaveSessionAsync();
            }
            finally
            {
                deferral.Complete();
            }
        }

        private async void OnAppEnteredBackground(object sender, Windows.ApplicationModel.EnteredBackgroundEventArgs e)
        {
            var deferral = e.GetDeferral();
            try
            {
                await ViewModel.SaveSessionAsync();
            }
            finally
            {
                deferral.Complete();
            }
        }

        private void DynamicPages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_isProcessingCollectionChange)
            {
                System.Diagnostics.Debug.WriteLine("DynamicPages_CollectionChanged: Already processing, skipping");
                return;
            }

            _isProcessingCollectionChange = true;
            System.Diagnostics.Debug.WriteLine($"DynamicPages_CollectionChanged: Action = {e.Action}");

            try
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (var item in e.NewItems)
                    {
                        if (item is EE_Calculator.Models.DynamicPageItem page)
                        {
                            System.Diagnostics.Debug.WriteLine($"DynamicPages_CollectionChanged: Adding UI item for {page.Title}");

                            var navItem = new WinUI.NavigationViewItem
                            {
                                Content = page.Title,
                                Tag = page.Id,
                                Icon = new SymbolIcon(Symbol.Calculator)
                            };

                            // Find the "Add Calculator Page" button and insert before it
                            int addButtonIndex = -1;
                            for (int i = 0; i < navigationView.MenuItems.Count; i++)
                            {
                                if (navigationView.MenuItems[i] is WinUI.NavigationViewItem item2 &&
                                    item2.Tag?.ToString() == "AddPage")
                                {
                                    addButtonIndex = i;
                                    break;
                                }
                            }

                            if (addButtonIndex >= 0)
                            {
                                navigationView.MenuItems.Insert(addButtonIndex, navItem);
                                System.Diagnostics.Debug.WriteLine($"DynamicPages_CollectionChanged: Inserted at index {addButtonIndex}");
                            }
                            else
                            {
                                navigationView.MenuItems.Add(navItem);
                                System.Diagnostics.Debug.WriteLine("DynamicPages_CollectionChanged: Added to end");
                            }
                        }
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (var item in e.OldItems)
                    {
                        if (item is EE_Calculator.Models.DynamicPageItem page)
                        {
                            // Remove the corresponding navigation item
                            WinUI.NavigationViewItem itemToRemove = null;
                            foreach (var navItem in navigationView.MenuItems)
                            {
                                if (navItem is WinUI.NavigationViewItem nvi && nvi.Tag is Guid id && id == page.Id)
                                {
                                    itemToRemove = nvi;
                                    break;
                                }
                            }

                            if (itemToRemove != null)
                            {
                                navigationView.MenuItems.Remove(itemToRemove);
                            }
                        }
                    }
                }
            }
            finally
            {
                _isProcessingCollectionChange = false;
                System.Diagnostics.Debug.WriteLine("DynamicPages_CollectionChanged: Finished");
            }
        }

        private void NavigationView_ItemInvoked(object sender, WinUI.NavigationViewItemInvokedEventArgs e)
        {
            // forward the event to the ViewModel so existing logic in ShellViewModel.OnItemInvoked is used
            ViewModel?.ItemInvokedCommand?.Execute(e);
        }

        private void Page_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel?.LoadedCommand?.Execute(null);
        }
    }
}

