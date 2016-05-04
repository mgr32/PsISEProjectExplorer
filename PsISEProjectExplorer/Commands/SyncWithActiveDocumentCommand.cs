
using PsISEProjectExplorer.UI.ViewModel;

namespace PsISEProjectExplorer.Commands
{
    [Component]
    public class SyncWithActiveDocumentCommand : ParameterizedCommand<bool>
    {
        private MainViewModel mainViewModel;

        private TreeViewModel treeViewModel;

        private LocateFileInTreeCommand locateFileInTreeCommand;

        private bool itemFound;

        public SyncWithActiveDocumentCommand(MainViewModel mainViewModel, TreeViewModel treeViewModel, LocateFileInTreeCommand locateFileInTreeCommand)
        {
            this.mainViewModel = mainViewModel;
            this.treeViewModel = treeViewModel;
            this.locateFileInTreeCommand = locateFileInTreeCommand;
        }

        public void Execute(bool resetState)
        {
            if (resetState)
            {
                this.itemFound = false;
            }
            if (this.mainViewModel.SyncWithActiveDocument && !this.itemFound)
            {
                this.itemFound = this.locateFileInTreeCommand.ExpandAndSelectCurrentlyOpenedItem();
            }
        }
    }
    
}
