using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace caro_game.Services
{
    /// <summary>
    /// Shows confirmations with ContentDialog in the main window.
    /// Only one ContentDialog may be open at a time, so a second request is answered with "cancel".
    /// </summary>
    public sealed class DialogService : IDialogService
    {
        private bool _isOpen;

        public async Task<bool> ConfirmAsync(string title, string message, string confirmText, string cancelText)
        {
            XamlRoot? xamlRoot = App.MainWindow?.Content?.XamlRoot;
            if (_isOpen || xamlRoot is null)
            {
                return false;
            }

            _isOpen = true;
            try
            {
                var dialog = new ContentDialog
                {
                    XamlRoot = xamlRoot,
                    Style = (Style)Application.Current.Resources["DefaultContentDialogStyle"],
                    Title = title,
                    Content = message,
                    PrimaryButtonText = confirmText,
                    CloseButtonText = cancelText,
                    DefaultButton = ContentDialogButton.Close
                };

                return await dialog.ShowAsync() == ContentDialogResult.Primary;
            }
            finally
            {
                _isOpen = false;
            }
        }
    }
}
