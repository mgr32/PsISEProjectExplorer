using PsISEProjectExplorer.UI.Helpers;
using PsISEProjectExplorer.UI.IseIntegration;
using PsISEProjectExplorer.UI.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.Commands
{
    public class LocateFileInTreeCommand : Command
    {
        private ProjectExplorerWindow ProjectExplorerWindow { get; set; }

        private IseIntegrator IseIntegrator { get; set; }

        private TreeViewModel TreeViewModel { get; set; }

        public LocateFileInTreeCommand(ProjectExplorerWindow projectExplorerWindow, IseIntegrator iseIntegrator, TreeViewModel treeViewModel)
        {
            this.ProjectExplorerWindow = projectExplorerWindow;
            this.IseIntegrator = iseIntegrator;
            this.TreeViewModel = treeViewModel;
        }

        public void Execute()
        {
            string path = this.IseIntegrator.SelectedFilePath;
            if (path == null)
            {
                return;
            }

            var selectedItem = this.ProjectExplorerWindow.SearchResultsTreeView.SelectedItem as TreeViewEntryItemModel;
            if (selectedItem != null && selectedItem.Path.StartsWith(path))
            {
                return;
            }

            TreeViewEntryItemModel item = this.TreeViewModel.FindTreeViewEntryItemByPath(path);
            if (item == null)
            {
                return;
            }

            this.ProjectExplorerWindow.SearchResultsTreeView.ExpandAndSelectItem(item);
        }
    }
}
