using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.UI.Helpers;
using PsISEProjectExplorer.UI.IseIntegration;
using PsISEProjectExplorer.UI.ViewModel;
using System.Linq;

namespace PsISEProjectExplorer.Commands
{
    [Component]
    public class UnsavedFileChecker
    {
        private readonly TreeViewModel treeViewModel;

        private readonly IseIntegrator iseIntegrator;

        private readonly MessageBoxHelper messageBoxHelper;

        public UnsavedFileChecker(TreeViewModel treeViewModel, IseIntegrator iseIntegrator, MessageBoxHelper messageBoxHelper)
        {
            this.treeViewModel = treeViewModel;
            this.iseIntegrator = iseIntegrator;
            this.messageBoxHelper = messageBoxHelper;
        }

        public bool EnsureCurrentlyOpenedFileIsSaved()
        {
            var selectedItem = this.treeViewModel.SelectedItem;
            if (selectedItem != null && selectedItem.NodeType == NodeType.File && this.iseIntegrator.OpenFiles.Contains(selectedItem.Path) && !this.iseIntegrator.IsFileSaved(selectedItem.Path))
            {
                this.iseIntegrator.GoToFile(selectedItem.Path);
                this.messageBoxHelper.ShowInfo("Please save your changes or close the file first.");
                return false;
            }
            return true;
        }
    }
}
