using PsISEProjectExplorer.UI.Helpers;

namespace PsISEProjectExplorer.Commands
{
    [Component]
    public class CollapseAllCommand : Command
    {
        private readonly ProjectExplorerWindow projectExplorerWindow;

        public CollapseAllCommand(ProjectExplorerWindow projectExplorerWindow)
        {
            this.projectExplorerWindow = projectExplorerWindow;
        }

        public void Execute()
        {
            this.projectExplorerWindow.SearchResultsTreeView.CollapseAll();
        }
    }
}
