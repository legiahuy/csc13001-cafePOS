using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CafePOS.Utilities
{
    public static class DialogHelper
    {
        /// <summary>
        /// Shows an error dialog with the specified title and content
        /// </summary>
        public static async Task ShowErrorDialog(string title, string content, XamlRoot xamlRoot)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "OK",
                XamlRoot = xamlRoot
            };
            await dialog.ShowAsync();
        }

        /// <summary>
        /// Shows a success dialog with the specified title and content
        /// </summary>
        public static async Task ShowSuccessDialog(string title, string content, XamlRoot xamlRoot)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "OK",
                XamlRoot = xamlRoot
            };
            await dialog.ShowAsync();
        }

        /// <summary>
        /// Shows a general message dialog
        /// </summary>
        public static async Task ShowMessageAsync(string message, XamlRoot xamlRoot)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Thông báo",
                Content = message,
                CloseButtonText = "Đóng",
                XamlRoot = xamlRoot
            };

            await dialog.ShowAsync();
        }

        /// <summary>
        /// Shows a confirmation dialog and returns the result
        /// </summary>
        public static async Task<ContentDialogResult> ShowConfirmDialog(string title, string content, string primaryButtonText, string closeButtonText, XamlRoot xamlRoot)
        {
            ContentDialog confirmDialog = new ContentDialog
            {
                Title = title,
                Content = content,
                PrimaryButtonText = primaryButtonText,
                CloseButtonText = closeButtonText,
                XamlRoot = xamlRoot
            };

            return await confirmDialog.ShowAsync();
        }
    }
} 