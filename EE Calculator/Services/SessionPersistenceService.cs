using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EE_Calculator.Controls;
using EE_Calculator.Helpers;
using EE_Calculator.Models;
using Windows.Storage;

namespace EE_Calculator.Services
{
    public static class SessionPersistenceService
    {
        private const string SessionDataKey = "SessionData";

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
                                    pageData.Tabs.Add(new TabData
                                    {
                                        Index = tab.Index,
                                        Header = tab.Header,
                                        MathInputText = calc.GetInputText()
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
                ApplicationData.Current.LocalSettings.Values.Remove(SessionDataKey);
                System.Diagnostics.Debug.WriteLine("SessionPersistenceService: Session cleared");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SessionPersistenceService: Error clearing session - {ex.Message}");
            }
            await Task.CompletedTask;
        }
    }
}
