using PsISEProjectExplorer.UI.Helpers;

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
