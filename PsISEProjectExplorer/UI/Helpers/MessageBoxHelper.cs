using System.Windows;

namespace PsISEProjectExplorer.UI.Helpers
{
    [Component]
    public class MessageBoxHelper
    {
        public void ShowError(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(Application.Current.MainWindow, message, "PsISEProjectExplorer - error", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }

        public void ShowInfo(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(Application.Current.MainWindow, message, "PsISEProjectExplorer - information", MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        public bool ShowConfirmMessage(string message)
        {
            return Application.Current.Dispatcher.Invoke(() =>
            {
                var result = MessageBox.Show(Application.Current.MainWindow, message, "Please confirm your action", MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel);
                return result == MessageBoxResult.OK;
            });
        }

        public bool ShowQuestion(string header, string message)
        {
            return Application.Current.Dispatcher.Invoke(() =>
            {
                var result = MessageBox.Show(Application.Current.MainWindow, message, "PsISEProjectExplorer - " + header, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                return result == MessageBoxResult.Yes;
            });
        }
    }
}
