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
        private object _selectedTab;

        public event EventHandler LastTabClosed;
        
        // Flag to indicate if this is the main page (shouldn't close when last tab closes)
        public bool IsMainPage { get; set; } = false;

        public RelayCommand AddTabCommand => _addTabCommand ?? (_addTabCommand = new RelayCommand(AddTab));

        public RelayCommand<WinUI.TabViewTabCloseRequestedEventArgs> CloseTabCommand => _closeTabCommand ?? (_closeTabCommand = new RelayCommand<WinUI.TabViewTabCloseRequestedEventArgs>(CloseTab));

        public ObservableCollection<TabViewItemData> Tabs { get; } = new ObservableCollection<TabViewItemData>();

        public object SelectedTab
        {
            get => _selectedTab;
            set => SetProperty(ref _selectedTab, value);
        }

        public MainViewModel()
        {
            // Constructor - tabs will be added by InitializeWithDefaultTab or RestoreFromTabData
        }

        public void InitializeWithDefaultTab()
        {
            if (Tabs.Count == 0)
            {
                AddTab();
            }
        }

        public void RestoreFromTabData(System.Collections.Generic.IEnumerable<TabData> tabDataList)
        {
            System.Diagnostics.Debug.WriteLine($"MainViewModel.RestoreFromTabData: Restoring {tabDataList?.Count() ?? 0} tabs");
            
            // Clear existing tabs
            Tabs.Clear();

            if (tabDataList != null && tabDataList.Any())
            {
                foreach (var tabData in tabDataList)
                {
                    var calculator = new CalculatorControl(showExampleText: false);
                    
                    var newTab = new TabViewItemData()
                    {
                        Index = tabData.Index,
                        Header = tabData.Header,
                        Content = calculator
                    };

                    Tabs.Add(newTab);
                    
                    // Set the input text after adding to collection
                    calculator.SetInputText(tabData.MathInputText);
                }

                // Select the first tab
                if (Tabs.Count > 0)
                {
                    SelectedTab = Tabs[0];
                }

                System.Diagnostics.Debug.WriteLine($"MainViewModel.RestoreFromTabData: Restored {Tabs.Count} tabs");
            }
            else
            {
                // No saved data, add default tab
                AddTab();
            }
        }

        public void RecreateTabControls()
        {
            System.Diagnostics.Debug.WriteLine($"MainViewModel.RecreateTabControls: Recreating UI controls for {Tabs.Count} tabs");
            
            // For each tab, get the current content (CalculatorControl), extract its data, and create a new control
            for (int i = 0; i < Tabs.Count; i++)
            {
                var tab = Tabs[i];
                
                // Only recreate if the content is a CalculatorControl
                if (tab.Content is CalculatorControl existingControl)
                {
                    // Get the current input text
                    string currentText = existingControl.GetInputText();
                    
                    // Create a new CalculatorControl with the same content
                    var newControl = new CalculatorControl(showExampleText: false);
                    newControl.SetInputText(currentText);
                    
                    // Replace the content
                    tab.Content = newControl;
                    
                    System.Diagnostics.Debug.WriteLine($"MainViewModel.RecreateTabControls: Recreated control for tab {tab.Index}");
                }
            }
        }

        private void AddTab()
        {
            int newIndex = Tabs.Any() ? Tabs.Max(t => t.Index) + 1 : 1;
            
            System.Diagnostics.Debug.WriteLine($"AddTab: Creating tab {newIndex}");
            
            // Show example text on every new tab
            var newTab = new TabViewItemData()
            {
                Index = newIndex,
                Header = $"Calculator {newIndex}",
                Content = new CalculatorControl(showExampleText: true)
            };
            
            Tabs.Add(newTab);
            System.Diagnostics.Debug.WriteLine($"AddTab: Tab added. Total tabs: {Tabs.Count}");
            
            // Automatically select the newly created tab
            SelectedTab = newTab;
            System.Diagnostics.Debug.WriteLine($"AddTab: Selected tab set to {newTab.Header}");
        }

        private void CloseTab(WinUI.TabViewTabCloseRequestedEventArgs args)
        {
            if (args.Item is TabViewItemData item)
            {
                // If this is the main page, keep at least one tab open
                if (IsMainPage && Tabs.Count <= 1)
                {
                    System.Diagnostics.Debug.WriteLine("CloseTab: Cannot close last tab on main page");
                    return;
                }

                // If this is a dynamic page and it's the last tab, notify to close the page
                if (!IsMainPage && Tabs.Count == 1)
                {
                    System.Diagnostics.Debug.WriteLine("CloseTab: Last tab closing on dynamic page, triggering page close");
                    Tabs.Remove(item);
                    LastTabClosed?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    Tabs.Remove(item);
                    System.Diagnostics.Debug.WriteLine($"CloseTab: Tab removed. Remaining tabs: {Tabs.Count}");
                }
            }
        }
    }
}

