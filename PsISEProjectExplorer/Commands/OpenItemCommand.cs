using PsISEProjectExplorer.UI.ViewModel;

namespace PsISEProjectExplorer.Commands
{
    public class OpenItemCommand : Command
    {
        private TreeViewModel TreeViewModel { get; set; }

        private MainViewModel MainViewModel { get; set; }

        public OpenItemCommand(TreeViewModel treeViewModel, MainViewModel mainViewModel)
        {
            this.TreeViewModel = treeViewModel;
            this.MainViewModel = mainViewModel;
        }

        public void Execute()
        {
            this.TreeViewModel.OpenItem(this.TreeViewModel.SelectedItem, this.MainViewModel.SearchOptions);
        }
    }
}
