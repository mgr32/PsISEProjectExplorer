using PsISEProjectExplorer.UI.Helpers;
using PsISEProjectExplorer.UI.ViewModel;
using System.Windows;

namespace PsISEProjectExplorer.Commands
{
    [Component]
    public class SelectItemCommand : ParameterizedCommand<DependencyObject>
    {
        private readonly ProjectExplorerWindow projectExplorerWindow;

        public SelectItemCommand(ProjectExplorerWindow projectExplorerWindow)
        {
            this.projectExplorerWindow = projectExplorerWindow;
        }

        public void Execute(DependencyObject originalSource)
        {
            var treeView = this.projectExplorerWindow.SearchResultsTreeView;
            var item = treeView.FindItemFromSource(originalSource);
            if (item == null && treeView.SelectedItem != null)
            {
                ((TreeViewEntryItemModel)treeView.SelectedItem).IsSelected = false;
            }
        }
    }
}
