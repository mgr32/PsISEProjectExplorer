using PsISEProjectExplorer.UI.ViewModel;

namespace PsISEProjectExplorer.Commands
{
    [Component]
    public class RenameItemCommand : Command
    {
        private readonly TreeViewModel treeViewModel;

        private readonly UnsavedFileChecker unsavedFileEnforcer;

        public RenameItemCommand(TreeViewModel treeViewModel, UnsavedFileChecker unsavedFileEnforcer)
        {
            this.treeViewModel = treeViewModel;
        }

        public void Execute()
        {
            var item = this.treeViewModel.SelectedItem;
            if (item == null)
            {
                return;
            }

            if (!this.unsavedFileEnforcer.EnsureCurrentlyOpenedFileIsSaved())
            {
                return;
            }
            item.IsBeingEdited = true;
        }
    }
}
