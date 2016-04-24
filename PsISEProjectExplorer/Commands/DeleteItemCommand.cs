using PsISEProjectExplorer.UI.ViewModel;

namespace PsISEProjectExplorer.Commands
{
    public class DeleteItemCommand : Command
    {
        private TreeViewModel TreeViewModel { get; set; }

        public DeleteItemCommand(TreeViewModel treeViewModel)
        {
            this.TreeViewModel = treeViewModel;
        }

        public void Execute()
        {
            var item = this.TreeViewModel.SelectedItem;
            if (item == null)
            {
                return;
            }

            this.TreeViewModel.DeleteTreeItem(item);
        }

    }
}
