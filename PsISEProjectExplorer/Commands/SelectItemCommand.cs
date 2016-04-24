using PsISEProjectExplorer.UI.Helpers;
using PsISEProjectExplorer.UI.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PsISEProjectExplorer.Commands
{
    public class SelectItemCommand : ParameterizedCommand<DependencyObject>
    {
        private ProjectExplorerWindow ProjectExplorerWindow { get; set; }

        public SelectItemCommand(ProjectExplorerWindow projectExplorerWindow)
        {
            this.ProjectExplorerWindow = projectExplorerWindow;
        }

        public void Execute(DependencyObject originalSource)
        {
            var treeView = this.ProjectExplorerWindow.SearchResultsTreeView;
            var item = treeView.FindItemFromSource(originalSource);
            if (item == null && treeView.SelectedItem != null)
            {
                ((TreeViewEntryItemModel)treeView.SelectedItem).IsSelected = false;
            }
        }
    }
}
