using System;

using EE_Calculator.Services;
using EE_Calculator.ViewModels;

using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace EE_Calculator.Views
{
    // TODO: Change the URL for your privacy policy in the Resource File, currently set to https://YourPrivacyUrlGoesHere
    public sealed partial class SettingsPage : Page
    {
        public SettingsViewModel ViewModel { get; } = new SettingsViewModel();

        public SettingsPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await ViewModel.InitializeAsync();
        }

        private async void ClearSessionButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Delete all saved calculator pages and restart?",
                Content = "This will delete all dynamic calculator pages and their tabs that are saved on this device. Theme and other app settings will not be changed. The app will restart. This action cannot be undone.",
                PrimaryButtonText = "Delete pages and restart app",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await SessionPersistenceService.ClearSessionAsync();

                // Request app restart after clearing session
                try
                {
                    await CoreApplication.RequestRestartAsync("User cleared saved calculator pages.");
                }
                catch
                {
                    // If restart is not allowed or fails, just exit
                    Windows.UI.Xaml.Application.Current.Exit();
                }
            }
        }
    }
}
