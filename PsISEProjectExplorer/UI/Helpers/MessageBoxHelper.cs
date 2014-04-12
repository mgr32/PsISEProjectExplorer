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
        public static void ShowMessage(string message)
        {
            MessageBox.Show(message);
        }

        public static bool ShowConfirmMessage(string message)
        {
            var result = MessageBox.Show(message, "Please confirm your action", MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel);
            return result == MessageBoxResult.OK;
        }
    }
}
