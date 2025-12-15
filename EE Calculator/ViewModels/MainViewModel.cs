using System;
using System.Collections.ObjectModel;
using System.Linq;
using EE_Calculator.Models;
using EE_Calculator.Controls;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using WinUI = Microsoft.UI.Xaml.Controls;

namespace EE_Calculator.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private RelayCommand _addTabCommand;
        private RelayCommand<WinUI.TabViewTabCloseRequestedEventArgs> _closeTabCommand;

        public event EventHandler LastTabClosed;

        public RelayCommand AddTabCommand => _addTabCommand ?? (_addTabCommand = new RelayCommand(AddTab));

        public RelayCommand<WinUI.TabViewTabCloseRequestedEventArgs> CloseTabCommand => _closeTabCommand ?? (_closeTabCommand = new RelayCommand<WinUI.TabViewTabCloseRequestedEventArgs>(CloseTab));

        public ObservableCollection<TabViewItemData> Tabs { get; } = new ObservableCollection<TabViewItemData>();

        public MainViewModel()
        {
            // Add initial tab
            AddTab();
        }

        private void AddTab()
        {
            int newIndex = Tabs.Any() ? Tabs.Max(t => t.Index) + 1 : 1;
            
            // Show example text on every new tab
            Tabs.Add(new TabViewItemData()
            {
                Index = newIndex,
                Header = $"Calculator {newIndex}",
                Content = new CalculatorControl(showExampleText: true)
            });
        }

        private void CloseTab(WinUI.TabViewTabCloseRequestedEventArgs args)
        {
            if (args.Item is TabViewItemData item)
            {
                if (Tabs.Count > 1)
                {
                    Tabs.Remove(item);
                }
                else if (Tabs.Count == 1)
                {
                    // Removing the last tab: close the entire page
                    Tabs.Remove(item);
                    LastTabClosed?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }
}
