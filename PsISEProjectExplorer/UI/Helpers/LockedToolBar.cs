using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PsISEProjectExplorer.UI.Helpers
{
    public class LockedToolBar : ToolBar
    {
        public LockedToolBar()
        {
            Loaded += new RoutedEventHandler(LockedToolBar_Loaded);
        }

        private void LockedToolBar_Loaded(object sender, RoutedEventArgs e)
        {
            ToolBar toolBar = sender as ToolBar;
            var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
            if (overflowGrid != null)
            {
                overflowGrid.Visibility = Visibility.Collapsed;
            }
        }
    }
}
