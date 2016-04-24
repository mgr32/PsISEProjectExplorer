using PsISEProjectExplorer.UI.ViewModel;

namespace PsISEProjectExplorer.Commands
{
    public class RenameItemCommand : Command
    {
        private TreeViewModel TreeViewModel { get; set; }

        public RenameItemCommand(TreeViewModel treeViewModel)
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

            this.TreeViewModel.StartEditingTreeItem(item);
        }
    }
}
