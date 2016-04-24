using PsISEProjectExplorer.UI.ViewModel;

namespace PsISEProjectExplorer.Commands
{
    public class RenameItemCommand : Command
    {
        private TreeViewModel TreeViewModel { get; set; }

        private UnsavedFileChecker UnsavedFileEnforcer { get; set; }

        public RenameItemCommand(TreeViewModel treeViewModel, UnsavedFileChecker unsavedFileEnforcer)
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

            if (!this.UnsavedFileEnforcer.EnsureCurrentlyOpenedFileIsSaved())
            {
                return;
            }
            item.IsBeingEdited = true;
        }
    }
}
