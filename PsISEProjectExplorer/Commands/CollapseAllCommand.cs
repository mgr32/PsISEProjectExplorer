using PsISEProjectExplorer.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.Commands
{
    public class CollapseAllCommand : Command
    {
        private ProjectExplorerWindow ProjectExplorerWindow { get; set; }

        public CollapseAllCommand(ProjectExplorerWindow projectExplorerWindow)
        {
            this.ProjectExplorerWindow = projectExplorerWindow;
        }

        public void Execute()
        {
            this.ProjectExplorerWindow.SearchResultsTreeView.CollapseAll();
        }
    }
}
