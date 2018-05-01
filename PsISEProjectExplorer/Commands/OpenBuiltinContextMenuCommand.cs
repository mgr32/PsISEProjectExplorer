using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.Services;
using PsISEProjectExplorer.UI.Helpers;
using PsISEProjectExplorer.UI.IseIntegration;
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

        private readonly FilesPatternProvider filesPatternProvider;

        private readonly IseIntegrator iseIntegrator;

        public OpenBuiltinContextMenuCommand(ProjectExplorerWindow projectExplorerWindow, FilesPatternProvider filesPatternProvider, IseIntegrator iseIntegrator)
        {
            this.projectExplorerWindow = projectExplorerWindow;
            this.filesPatternProvider = filesPatternProvider;
            this.iseIntegrator = iseIntegrator;
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
                        treeView.ContextMenu = treeView.Resources["PowershellItemContext"] as ContextMenu;
                        break;
                }
            
            if (item.NodeType == NodeType.Intermediate)
            {
                treeView.ContextMenu = null;
            }
            if (treeView.ContextMenu != null)
            {
                SetIncludeAndExcludeVisibility(treeView, item);
                SetDotSourceVisibility(treeView, item);
            }
        }

        private void SetDotSourceVisibility(StretchingTreeView treeView, TreeViewEntryItemModel item)
        {
            MenuItem dotSourceMenuItem = this.FindMenuItem(treeView.ContextMenu.Items, "Dot Source");
            if (dotSourceMenuItem != null)
            {
                dotSourceMenuItem.Visibility = item.NodeType == NodeType.Directory || item.NodeType == NodeType.Intermediate || iseIntegrator.SelectedFilePath == null
                    ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private void SetIncludeAndExcludeVisibility(StretchingTreeView treeView, TreeViewEntryItemModel item)
        {
            MenuItem includeMenuItem = this.FindMenuItem(treeView.ContextMenu.Items, "Include");
            MenuItem excludeMenuItem = this.FindMenuItem(treeView.ContextMenu.Items, "Exclude");
            if (includeMenuItem != null && excludeMenuItem != null)
            {
                if (item.IsExcluded)
                {
                    includeMenuItem.Visibility = filesPatternProvider.ExcludePaths.Contains(item.Path) ? Visibility.Visible : Visibility.Collapsed;
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
