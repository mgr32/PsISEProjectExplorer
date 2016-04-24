using PsISEProjectExplorer.UI.ViewModel;

namespace PsISEProjectExplorer.Commands
{
    public class FindInFilesCommand : Command
    {
        private MainViewModel MainViewModel { get; set; }

        private ProjectExplorerWindow ProjectExplorerWindow { get; set; }

        public FindInFilesCommand(MainViewModel mainViewModel, ProjectExplorerWindow projectExplorerWindow)
        {
            this.MainViewModel = mainViewModel;
            this.ProjectExplorerWindow = projectExplorerWindow;
        }

        public void Execute()
        {
            this.MainViewModel.SearchOptions.SearchText = string.Empty;
            this.MainViewModel.SearchInFiles = true;

            this.ProjectExplorerWindow.FocusOnTextBoxSearchText();
        }
    }
}
