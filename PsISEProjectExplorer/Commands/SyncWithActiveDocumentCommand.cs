
using PsISEProjectExplorer.UI.ViewModel;

namespace PsISEProjectExplorer.Commands
{
    [Component]
    public class SyncWithActiveDocumentCommand : Command
    {
        private MainViewModel mainViewModel;

        private LocateFileInTreeCommand locateFileInTreeCommand;

        public SyncWithActiveDocumentCommand(MainViewModel mainViewModel, LocateFileInTreeCommand locateFileInTreeCommand)
        {
            this.mainViewModel = mainViewModel;
            this.locateFileInTreeCommand = locateFileInTreeCommand;
        }

        public void Execute()
        {
            if (this.mainViewModel.SyncWithActiveDocument)
            {
                // TODO: this should be suppressed during indexing
                this.locateFileInTreeCommand.Execute();
            }
        }
    }
    
}
