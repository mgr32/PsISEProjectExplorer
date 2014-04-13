using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PsISEProjectExplorer.UI.Helpers
{
    // http://blogs.msdn.com/b/jpricket/archive/2008/08/05/wpf-a-stretching-treeview.aspx
    public class StretchingTreeView : TreeView
    {
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new StretchingTreeViewItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is StretchingTreeViewItem;
        }
    }

    class StretchingTreeViewItem : TreeViewItem
    {
        public StretchingTreeViewItem()
        {
            this.Loaded += new RoutedEventHandler(StretchingTreeViewItem_Loaded);
        }

        private void StretchingTreeViewItem_Loaded(object sender, RoutedEventArgs e)
        {
            // The purpose of this code is to stretch the Header Content all the way accross the TreeView. 
            if (this.VisualChildrenCount > 0)
            {
                Grid grid = this.GetVisualChild(0) as Grid;
                if (grid != null && grid.ColumnDefinitions.Count == 3)
                {
                    // Remove the middle column which is set to Auto and let it get replaced with the 
                    // last column that is set to Star.
                    grid.ColumnDefinitions.RemoveAt(1);
                }
            }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new StretchingTreeViewItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is StretchingTreeViewItem;
        }
    }
}
