using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EE_Calculator.Controls;
using EE_Calculator.Helpers;
using EE_Calculator.Models;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;

namespace EE_Calculator.Services
{
    public static class SessionPersistenceService
    {
        private const string SessionDataKey = "SessionData";

        // Helper to normalize text for comparison (line endings and trailing whitespace)
        private static string NormalizeText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            // Normalize CRLF to LF and trim trailing whitespace
            return text.Replace("\r\n", "\n").Replace("\r", "\n").TrimEnd();
        }

        public static async Task SaveSessionAsync(
            IEnumerable<TabViewItemData> mainPageTabs,
            IEnumerable<DynamicPageItem> dynamicPages,
            int pageCounter)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("SessionPersistenceService: Saving session...");

                var sessionData = new SessionData
                {
                    PageCounter = pageCounter
                };

                // Do NOT save main page tabs - Main page always starts fresh with one tab
                System.Diagnostics.Debug.WriteLine($"SessionPersistenceService: Skipping main page tabs (Main page does not persist)");

                // Save dynamic pages and their tabs
                if (dynamicPages != null)
                {
                    foreach (var page in dynamicPages)
                    {
                        var pageData = new DynamicPageData
                        {
                            Id = page.Id,
                            Title = page.Title
                        };

                        // Get the cached ViewModel for this page
                        var viewModel = Views.CalculatorPage.GetCachedViewModel(page.Id);
                        if (viewModel != null && viewModel.Tabs != null)
                        {
                            foreach (var tab in viewModel.Tabs)
                            {
                                if (tab.Content is CalculatorControl calc)
                                {
                                    var text = calc.GetInputText() ?? string.Empty;

                                    pageData.Tabs.Add(new TabData
                                    {
                                        Index = tab.Index,
                                        Header = tab.Header,
                                        MathInputText = text
                                    });
                                }
                            }
                        }

                        sessionData.DynamicPages.Add(pageData);
                        System.Diagnostics.Debug.WriteLine($"SessionPersistenceService: Saved page '{page.Title}' with {pageData.Tabs.Count} tabs");
                    }
                }

                // Save to local storage
                await ApplicationData.Current.LocalSettings.SaveAsync(SessionDataKey, sessionData);
                System.Diagnostics.Debug.WriteLine("SessionPersistenceService: Session saved successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SessionPersistenceService: Error saving session - {ex.Message}");
            }
        }

        public static async Task<SessionData> LoadSessionAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("SessionPersistenceService: Loading session...");
                var sessionData = await ApplicationData.Current.LocalSettings.ReadAsync<SessionData>(SessionDataKey);

                if (sessionData != null)
                {
                    System.Diagnostics.Debug.WriteLine($"SessionPersistenceService: Loaded session with {sessionData.MainPageTabs.Count} main tabs and {sessionData.DynamicPages.Count} dynamic pages");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("SessionPersistenceService: No saved session found");
                }

                return sessionData;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SessionPersistenceService: Error loading session - {ex.Message}");
                return null;
            }
        }

        public static async Task ClearSessionAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("SessionPersistenceService: Clearing calculator session data only...");

                // Remove the persisted calculator session blob, keep other app settings (e.g., theme)
                ApplicationData.Current.LocalSettings.Values.Remove(SessionDataKey);

                // Also clear in-memory state so current session matches what will be loaded next time
                var shellPage = Window.Current.Content as EE_Calculator.Views.ShellPage;
                if (shellPage != null)
                {
                    // Clear dynamic pages collection and reset its page counter
                    shellPage.ViewModel.DynamicPages.Clear();

                    // Reset the internal page counter so new pages start from 1 again
                    shellPage.ViewModel.ResetPageCounter();
                }

                EE_Calculator.Views.CalculatorPage.ClearAllCachedViewModels();

                System.Diagnostics.Debug.WriteLine("SessionPersistenceService: Calculator session data cleared (persisted + in-memory)");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SessionPersistenceService: Error clearing session - {ex.Message}");
            }

            await Task.CompletedTask;
        }

        // Export only dynamic calculator pages and their tabs to a JSON file selected by the user
        public static async Task ExportDynamicPagesAsync()
        {
            try
            {
                // Flush current in-memory state to persistent session first
                var shellPage = Window.Current.Content as EE_Calculator.Views.ShellPage;
                if (shellPage != null)
                {
                    await shellPage.ViewModel.SaveSessionAsync();
                }

                var session = await LoadSessionAsync();
                if (session == null)
                {
                    System.Diagnostics.Debug.WriteLine("SessionPersistenceService.ExportDynamicPagesAsync: No session to export");
                    return;
                }

                // Clone the session data for export so we can sanitize it
                var exportData = new SessionData
                {
                    PageCounter = session.PageCounter
                };

                var normalizedWelcome = NormalizeText(CalculatorControl.InitialWelcomeText);

                foreach (var page in session.DynamicPages)
                {
                    var pageCopy = new DynamicPageData
                    {
                        Id = page.Id,
                        Title = page.Title
                    };

                    foreach (var tab in page.Tabs)
                    {
                        var text = tab.MathInputText ?? string.Empty;

                        // If this tab still contains the initial welcome/example text (after normalization),
                        // replace it with an empty string in the exported file.
                        if (string.Equals(NormalizeText(text), normalizedWelcome, StringComparison.Ordinal))
                        {
                            text = string.Empty;
                        }

                        pageCopy.Tabs.Add(new TabData
                        {
                            Index = tab.Index,
                            Header = tab.Header,
                            MathInputText = text
                        });
                    }

                    exportData.DynamicPages.Add(pageCopy);
                }

                var picker = new FileSavePicker
                {
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary
                };
                picker.FileTypeChoices.Add("EE Calculator session", new List<string> { ".json" });
                picker.SuggestedFileName = "ee_calculator_session";

#pragma warning disable CS0618
                StorageFile file = await picker.PickSaveFileAsync();
#pragma warning restore CS0618
                if (file == null)
                {
                    return;
                }

                var json = await EE_Calculator.Core.Helpers.Json.StringifyAsync(exportData);
                await FileIO.WriteTextAsync(file, json);

                System.Diagnostics.Debug.WriteLine($"SessionPersistenceService.ExportDynamicPagesAsync: Exported to {file.Path}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SessionPersistenceService.ExportDynamicPagesAsync: Error - {ex.Message}");
            }
        }

        // Import dynamic pages and their tabs from a JSON file and persist/apply them as current session
        public static async Task ImportDynamicPagesAsync()
        {
            try
            {
                var picker = new FileOpenPicker
                {
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                    ViewMode = PickerViewMode.List
                };
                picker.FileTypeFilter.Add(".json");

#pragma warning disable CS0618
                var file = await picker.PickSingleFileAsync();
#pragma warning restore CS0618
                if (file == null)
                {
                    return;
                }

                var json = await FileIO.ReadTextAsync(file);
                var imported = await EE_Calculator.Core.Helpers.Json.ToObjectAsync<SessionData>(json);
                if (imported == null)
                {
                    System.Diagnostics.Debug.WriteLine("SessionPersistenceService.ImportDynamicPagesAsync: Failed to deserialize session data");
                    return;
                }

                // Persist imported data as the new session
                await ApplicationData.Current.LocalSettings.SaveAsync(SessionDataKey, imported);

                // Apply imported session to current in-memory state immediately
                var shellPage = Window.Current.Content as EE_Calculator.Views.ShellPage;
                if (shellPage != null)
                {
                    shellPage.ViewModel.InitializeFromSession(imported);
                }

                System.Diagnostics.Debug.WriteLine("SessionPersistenceService.ImportDynamicPagesAsync: Import applied to current session, requesting restart");

                // Restart the app so subsequent launches start from the imported session only
                try
                {
                    await CoreApplication.RequestRestartAsync("User imported calculator session.");
                }
                catch
                {
                    Application.Current.Exit();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SessionPersistenceService.ImportDynamicPagesAsync: Error - {ex.Message}");
            }
        }
    }
}
