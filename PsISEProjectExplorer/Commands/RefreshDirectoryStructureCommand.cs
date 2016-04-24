using PsISEProjectExplorer.UI.ViewModel;

namespace PsISEProjectExplorer.Commands
{
    public class RefreshDirectoryStructureCommand : Command
    {
        private MainViewModel MainViewModel { get; set; }

        public RefreshDirectoryStructureCommand(MainViewModel mainViewModel)
        {
            this.MainViewModel = mainViewModel;
        }

        public void Execute()
        {
            this.MainViewModel.ReindexSearchTree();
        }
    }
}
