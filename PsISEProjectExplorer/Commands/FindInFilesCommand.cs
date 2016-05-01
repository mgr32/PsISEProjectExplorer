using PsISEProjectExplorer.UI.ViewModel;

namespace PsISEProjectExplorer.Commands
{
    [Component]
    public class FindInFilesCommand : Command
    {
        private readonly MainViewModel mainViewModel;

        private readonly ProjectExplorerWindow projectExplorerWindow;

        public FindInFilesCommand(MainViewModel mainViewModel, ProjectExplorerWindow projectExplorerWindow)
        {
            this.mainViewModel = mainViewModel;
            this.projectExplorerWindow = projectExplorerWindow;
        }

        public void Execute()
        {
            this.mainViewModel.SearchOptions.SearchText = string.Empty;
            this.mainViewModel.SearchInFiles = true;

            this.projectExplorerWindow.FocusOnTextBoxSearchText();
        }
    }
}
