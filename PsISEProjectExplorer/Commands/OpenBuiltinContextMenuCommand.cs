using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.UI.Helpers;
using PsISEProjectExplorer.UI.ViewModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PsISEProjectExplorer.Commands
{
    [Component]
    public class OpenBuiltinContextMenuCommand : ParameterizedCommand<DependencyObject>
    {
        private readonly ProjectExplorerWindow projectExplorerWindow;

        public OpenBuiltinContextMenuCommand(ProjectExplorerWindow projectExplorerWindow)
        {
            this.projectExplorerWindow = projectExplorerWindow;
        }

        public void Execute(DependencyObject originalSource)
        {
            var treeView = this.projectExplorerWindow.SearchResultsTreeView;
            TreeViewEntryItemModel item;
            if (!treeView.SelectItemFromSource(originalSource))
            {
                treeView.ContextMenu = treeView.Resources["EmptyContext"] as ContextMenu;

                item = (TreeViewEntryItemModel)treeView.SelectedItem;
                if (item != null)
                {
                    item.IsSelected = false;
                }

                return;
            }

            item = (TreeViewEntryItemModel)treeView.SelectedItem;
            if (item == null)
            {
                // should not happen
                treeView.ContextMenu = null;
            }
            else
                switch (item.NodeType)
                {
                    case NodeType.Directory:
                        treeView.ContextMenu = treeView.Resources["DirectoryContext"] as ContextMenu;
                        break;
                    case NodeType.File:
                        treeView.ContextMenu = treeView.Resources["FileContext"] as ContextMenu;
                        break;
                    default:
                        treeView.ContextMenu = null;
                        break;
                }

            if (treeView.ContextMenu != null)
            {
                MenuItem includeMenuItem = this.FindMenuItem(treeView.ContextMenu.Items, "Include");
                MenuItem excludeMenuItem = this.FindMenuItem(treeView.ContextMenu.Items, "Exclude");

                if (item.IsExcluded)
                {
                    includeMenuItem.Visibility = Visibility.Visible;
                    excludeMenuItem.Visibility = Visibility.Collapsed;
                }
                else
                {
                    includeMenuItem.Visibility = Visibility.Collapsed;
                    excludeMenuItem.Visibility = Visibility.Visible;
                }
            }
        }

        private MenuItem FindMenuItem(ItemCollection itemCollection, string header)
        {
            return (MenuItem)itemCollection.Cast<object>().Where(item => item is MenuItem && ((MenuItem)item).Header.ToString() == header).FirstOrDefault();
        }
    }
}
