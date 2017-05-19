using PsISEProjectExplorer.UI.ViewModel;

namespace PsISEProjectExplorer.Commands
{
    [Component]
    public class RenameItemCommand : Command
    {
        private readonly TreeViewModel treeViewModel;

        private readonly UnsavedFileChecker unsavedFileChecker;

        public RenameItemCommand(TreeViewModel treeViewModel, UnsavedFileChecker unsavedFileChecker)
        {
            this.treeViewModel = treeViewModel;
            this.unsavedFileChecker = unsavedFileChecker;
        }

        public void Execute()
        {
            var item = this.treeViewModel.SelectedItem;
            if (item == null)
            {
                return;
            }

            if (!this.unsavedFileChecker.EnsureCurrentlyOpenedFileIsSaved())
            {
                return;
            }
            item.IsBeingEdited = true;
        }
    }
}
