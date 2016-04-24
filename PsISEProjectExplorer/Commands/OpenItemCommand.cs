using PsISEProjectExplorer.UI.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.Commands
{
    public class OpenItemCommand : Command
    {
        private ProjectExplorerWindow ProjectExplorerWindow { get; set; }

        private TreeViewModel TreeViewModel { get; set; }

        private MainViewModel MainViewModel { get; set; }

        public OpenItemCommand(ProjectExplorerWindow projectExplorerWindow, TreeViewModel treeViewModel, MainViewModel mainViewModel)
        {
            this.ProjectExplorerWindow = projectExplorerWindow;
            this.TreeViewModel = treeViewModel;
            this.MainViewModel = mainViewModel;
        }

        public void Execute()
        {
            this.TreeViewModel.OpenItem((TreeViewEntryItemModel)this.ProjectExplorerWindow.SearchResultsTreeView.SelectedItem, this.MainViewModel.SearchOptions);
        }
    }
}
