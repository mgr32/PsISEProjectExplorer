using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PsISEProjectExplorer.UI.Helpers
{
    public static class MessageBoxHelper
    {
        public static void ShowError(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(Application.Current.MainWindow, message, "PsISEProjectExplorer - error", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }

        public static void ShowInfo(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(Application.Current.MainWindow, message, "PsISEProjectExplorer - information", MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        public static bool ShowConfirmMessage(string message)
        {
            return Application.Current.Dispatcher.Invoke(() =>
            {
                var result = MessageBox.Show(Application.Current.MainWindow, message, "Please confirm your action", MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel);
                return result == MessageBoxResult.OK;
            });
        }

        public static bool ShowQuestion(string header, string message)
        {
            return Application.Current.Dispatcher.Invoke(() =>
            {
                var result = MessageBox.Show(Application.Current.MainWindow, message, "PsISEProjectExplorer - " + header, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                return result == MessageBoxResult.Yes;
            });
        }
    }
}
