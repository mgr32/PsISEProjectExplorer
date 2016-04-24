using PsISEProjectExplorer.UI.ViewModel;

namespace PsISEProjectExplorer.Commands
{
    public class ExcludeOrIncludeItemCommand : Command
    {

        private MainViewModel MainViewModel { get; set; }

        private TreeViewModel TreeViewModel { get; set; }

        public ExcludeOrIncludeItemCommand(MainViewModel mainViewModel, TreeViewModel treeViewModel)
        {
            this.MainViewModel = mainViewModel;
            this.TreeViewModel = treeViewModel;
        }

        public void Execute()
        {
            var item = this.TreeViewModel.SelectedItem;
            if (item == null || item.IsBeingEdited)
            {
                return;
            }
            this.MainViewModel.ExcludeOrIncludeItem(item);
        }
    }
}
