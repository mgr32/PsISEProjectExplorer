using PsISEProjectExplorer.UI.Helpers;
using PsISEProjectExplorer.UI.IseIntegration;
using PsISEProjectExplorer.UI.ViewModel;

namespace PsISEProjectExplorer.Commands
{
    [Component]
    public class LocateFileInTreeCommand : Command
    {
        private readonly ProjectExplorerWindow projectExplorerWindow;

        private readonly IseIntegrator iseIntegrator;

        private readonly TreeViewModel treeViewModel;

        public LocateFileInTreeCommand(ProjectExplorerWindow projectExplorerWindow, IseIntegrator iseIntegrator, TreeViewModel treeViewModel)
        {
            this.projectExplorerWindow = projectExplorerWindow;
            this.iseIntegrator = iseIntegrator;
            this.treeViewModel = treeViewModel;
        }

        public void Execute()
        {
            this.ExpandAndSelectCurrentlyOpenedItem();
        }

        public bool ExpandAndSelectCurrentlyOpenedItem()
        {
            string path = this.iseIntegrator.SelectedFilePath;
            if (path == null)
            {
                return false;
            }

            var selectedItem = this.treeViewModel.SelectedItem;
            if (selectedItem != null && selectedItem.Path.StartsWith(path))
            {
                return true;
            }

            TreeViewEntryItemModel item = this.treeViewModel.FindTreeViewEntryItemByPath(path);
            if (item == null)
            {
                return false;
            }

            this.projectExplorerWindow.SearchResultsTreeView.ExpandAndSelectItem(item);
            return true;
        }
    }
}
