using PsISEProjectExplorer.UI.ViewModel;

namespace PsISEProjectExplorer.Commands
{
    public class EndEditingTreeItemCommand : ParameterizedCommand<string>
    {

        private MainViewModel MainViewModel { get; set; }

        private TreeViewModel TreeViewModel { get; set; }

        public EndEditingTreeItemCommand(MainViewModel mainViewModel, TreeViewModel treeViewModel)
        {
            this.MainViewModel = mainViewModel;
            this.TreeViewModel = treeViewModel;
        }

        public void Execute(string newValue)
        {
            var item = this.TreeViewModel.SelectedItem;
            if (item == null)
            {
                return;
            }

            this.MainViewModel.TreeViewModel.EndTreeEdit(newValue, true, item, !this.MainViewModel.SearchInFiles);
        }
    }
}
