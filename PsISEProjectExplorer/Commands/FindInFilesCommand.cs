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
            if (this.mainViewModel.IndexFilesMode == Model.IndexingMode.NO_FILES)
            {
                this.mainViewModel.IndexFilesMode = Model.IndexingMode.LOCAL_FILES;
            }
            this.projectExplorerWindow.FocusOnTextBoxSearchText();
        }
    }
}
