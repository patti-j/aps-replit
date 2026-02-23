using DevExpress.Blazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ReportsWebApp.Common
{
    public static class ToastExtensions
    {
        /// <summary>
        /// Method <c>Replace</c> replaces the first element that matches replaceBy
        /// </summary>
        /// <param name="title">The title of the toast notification. Pass an empty string for a toast with no title.</param>
        /// <param name="message">The message body of the toast notification.</param>
        /// <param name="style">The style in which to display the toast notification.</param>
        /// <param name="displayForSeconds">The amount of seconds for which the notification should be shown. If set to 0, the notification will not close automatically.</param>
        public static void ShowToast(this IToastNotificationService toastService, string title, string message, ToastRenderStyle style, int displayForSeconds = 10)
        {
            toastService.ShowToast(new ToastOptions
            {
                DisplayTime = TimeSpan.FromSeconds(displayForSeconds),
                ProviderName = "MainLayout",
                Title = title,
                Text = message,
                ThemeMode = ToastThemeMode.Saturated,
                RenderStyle = style,
            });
        }
    }

    
}
