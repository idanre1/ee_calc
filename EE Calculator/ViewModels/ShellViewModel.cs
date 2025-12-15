using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using EE_Calculator.Helpers;
using EE_Calculator.Models;
using EE_Calculator.Services;
using EE_Calculator.Views;

using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

using WinUI = Microsoft.UI.Xaml.Controls;

namespace EE_Calculator.ViewModels
{
    public class ShellViewModel : ObservableObject
    {
        private readonly KeyboardAccelerator _altLeftKeyboardAccelerator = BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu);
        private readonly KeyboardAccelerator _backKeyboardAccelerator = BuildKeyboardAccelerator(VirtualKey.GoBack);

        private bool _isBackEnabled;
        private IList<KeyboardAccelerator> _keyboardAccelerators;
        private WinUI.NavigationView _navigationView;
        private WinUI.NavigationViewItem _selected;
        private ICommand _loadedCommand;
        private ICommand _itemInvokedCommand;
        private ICommand _addPageCommand;
        private int _pageCounter = 1;
        private bool _isAddingPage = false;
        private Guid? _currentPageId;

        public ObservableCollection<DynamicPageItem> DynamicPages { get; } = new ObservableCollection<DynamicPageItem>();

        public bool IsBackEnabled
        {
            get { return _isBackEnabled; }
            set { SetProperty(ref _isBackEnabled, value); }
        }

        public WinUI.NavigationViewItem Selected
        {
            get { return _selected; }
            set { SetProperty(ref _selected, value); }
        }

        public ICommand LoadedCommand => _loadedCommand ?? (_loadedCommand = new RelayCommand(OnLoaded));

        public ICommand ItemInvokedCommand => _itemInvokedCommand ?? (_itemInvokedCommand = new RelayCommand<WinUI.NavigationViewItemInvokedEventArgs>(OnItemInvoked));

        public ICommand AddPageCommand => _addPageCommand ?? (_addPageCommand = new RelayCommand(AddNewPage));

        public ShellViewModel()
        {
        }

        public void Initialize(Frame frame, WinUI.NavigationView navigationView, IList<KeyboardAccelerator> keyboardAccelerators)
        {
            _navigationView = navigationView;
            _keyboardAccelerators = keyboardAccelerators;
            NavigationService.Frame = frame;
            NavigationService.NavigationFailed += Frame_NavigationFailed;
            NavigationService.Navigated += Frame_Navigated;
            _navigationView.BackRequested += OnBackRequested;
        }

        private async void OnLoaded()
        {
            // Keyboard accelerators are added here to avoid showing 'Alt + left' tooltip on the page.
            // More info on tracking issue https://github.com/Microsoft/microsoft-ui-xaml/issues/8
            _keyboardAccelerators.Add(_altLeftKeyboardAccelerator);
            _keyboardAccelerators.Add(_backKeyboardAccelerator);
            await Task.CompletedTask;
        }

        private void AddNewPage()
        {
            // Prevent double-clicks or duplicate invocations
            if (_isAddingPage)
            {
                System.Diagnostics.Debug.WriteLine("AddNewPage: Already adding a page, skipping");
                return;
            }

            _isAddingPage = true;
            System.Diagnostics.Debug.WriteLine($"AddNewPage: Creating Calculator {_pageCounter}");

            try
            {
                var newPage = new DynamicPageItem
                {
                    Id = Guid.NewGuid(),
                    Title = $"Calculator {_pageCounter}",
                    PageType = typeof(CalculatorPage),
                    IsClosable = true
                };

                System.Diagnostics.Debug.WriteLine($"AddNewPage: Adding page to collection - {newPage.Title} ({newPage.Id})");
                DynamicPages.Add(newPage);
                _pageCounter++;

                // Navigate to the new page
                System.Diagnostics.Debug.WriteLine($"AddNewPage: Navigating to {newPage.Title}");
                NavigationService.Navigate(typeof(CalculatorPage), newPage.Id);
            }
            finally
            {
                _isAddingPage = false;
                System.Diagnostics.Debug.WriteLine("AddNewPage: Finished");
            }
        }

        public void ClosePage(Guid pageId)
        {
            var page = DynamicPages.FirstOrDefault(p => p.Id == pageId);
            if (page != null)
            {
                DynamicPages.Remove(page);
            }

            // If this page is currently displayed, navigate away
            if (_currentPageId.HasValue && _currentPageId.Value == pageId)
            {
                if (!NavigationService.GoBack())
                {
                    NavigationService.Navigate(typeof(MainPage));
                }
            }
        }

        public string GetPageTitle(Guid pageId)
        {
            var page = DynamicPages.FirstOrDefault(p => p.Id == pageId);
            return page?.Title;
        }

        public void RenamePage(Guid pageId, string newTitle)
        {
            if (string.IsNullOrWhiteSpace(newTitle))
            {
                return;
            }

            var page = DynamicPages.FirstOrDefault(p => p.Id == pageId);
            if (page == null)
            {
                return;
            }

            page.Title = newTitle;

            // Update corresponding navigation item header
            var navItem = GetDynamicMenuItem(pageId);
            if (navItem != null)
            {
                navItem.Content = newTitle;
            }
        }

        private void OnItemInvoked(WinUI.NavigationViewItemInvokedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine($"OnItemInvoked: Called");
            
            if (args.IsSettingsInvoked)
            {
                NavigationService.Navigate(typeof(SettingsPage), null, args.RecommendedNavigationTransitionInfo);
            }
            else
            {
                var selectedItem = args.InvokedItemContainer as WinUI.NavigationViewItem;
                System.Diagnostics.Debug.WriteLine($"OnItemInvoked: Item tag = {selectedItem?.Tag}");
                
                // Check if this is the add page button
                if (selectedItem?.Tag?.ToString() == "AddPage")
                {
                    System.Diagnostics.Debug.WriteLine("OnItemInvoked: Add page button clicked");
                    AddNewPage();
                    return;
                }

                // Check if this is a dynamic page
                if (selectedItem?.Tag is Guid pageId)
                {
                    var dynamicPage = DynamicPages.FirstOrDefault(p => p.Id == pageId);
                    if (dynamicPage != null)
                    {
                        NavigationService.Navigate(dynamicPage.PageType, pageId, args.RecommendedNavigationTransitionInfo);
                        return;
                    }
                }

                var pageType = selectedItem?.GetValue(NavHelper.NavigateToProperty) as Type;

                if (pageType != null)
                {
                    NavigationService.Navigate(pageType, null, args.RecommendedNavigationTransitionInfo);
                }
            }
        }

        private void OnBackRequested(WinUI.NavigationView sender, WinUI.NavigationViewBackRequestedEventArgs args)
        {
            NavigationService.GoBack();
        }

        private void Frame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw e.Exception;
        }

        private void Frame_Navigated(object sender, NavigationEventArgs e)
        {
            IsBackEnabled = NavigationService.CanGoBack;
            _currentPageId = null;

            if (e.SourcePageType == typeof(SettingsPage))
            {
                Selected = _navigationView.SettingsItem as WinUI.NavigationViewItem;
                return;
            }

            // Check if navigating to a dynamic page
            if (e.SourcePageType == typeof(CalculatorPage) && e.Parameter is Guid pageId)
            {
                _currentPageId = pageId;

                var dynamicMenuItem = GetDynamicMenuItem(pageId);
                if (dynamicMenuItem != null)
                {
                    Selected = dynamicMenuItem;
                    return;
                }
            }

            var selectedItem = GetSelectedItem(_navigationView.MenuItems, e.SourcePageType);
            if (selectedItem != null)
            {
                Selected = selectedItem;
            }
        }

        private WinUI.NavigationViewItem GetDynamicMenuItem(Guid pageId)
        {
            foreach (var item in _navigationView.MenuItems.OfType<WinUI.NavigationViewItem>())
            {
                if (item.Tag is Guid id && id == pageId)
                {
                    return item;
                }
            }
            return null;
        }

        private WinUI.NavigationViewItem GetSelectedItem(IEnumerable<object> menuItems, Type pageType)
        {
            foreach (var item in menuItems.OfType<WinUI.NavigationViewItem>())
            {
                if (IsMenuItemForPageType(item, pageType))
                {
                    return item;
                }

                var selectedChild = GetSelectedItem(item.MenuItems, pageType);
                if (selectedChild != null)
                {
                    return selectedChild;
                }
            }

            return null;
        }

        private bool IsMenuItemForPageType(WinUI.NavigationViewItem menuItem, Type sourcePageType)
        {
            var pageType = menuItem.GetValue(NavHelper.NavigateToProperty) as Type;
            return pageType == sourcePageType;
        }

        private static KeyboardAccelerator BuildKeyboardAccelerator(VirtualKey key, VirtualKeyModifiers? modifiers = null)
        {
            var keyboardAccelerator = new KeyboardAccelerator() { Key = key };
            if (modifiers.HasValue)
            {
                keyboardAccelerator.Modifiers = modifiers.Value;
            }

            keyboardAccelerator.Invoked += OnKeyboardAcceleratorInvoked;
            return keyboardAccelerator;
        }

        private static void OnKeyboardAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            var result = NavigationService.GoBack();
            args.Handled = result;
        }
    }
}
