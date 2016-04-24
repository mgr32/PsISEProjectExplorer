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
        private TreeViewModel TreeViewModel { get; set; }
        
        private IseIntegrator IseIntegrator { get; set; }

        private MessageBoxHelper MessageBoxHelper { get; set; }

        public UnsavedFileChecker(TreeViewModel treeViewModel, IseIntegrator iseIntegrator, MessageBoxHelper messageBoxHelper)
        {
            this.TreeViewModel = treeViewModel;
            this.IseIntegrator = iseIntegrator;
            this.MessageBoxHelper = messageBoxHelper;
        }

        public bool EnsureCurrentlyOpenedFileIsSaved()
        {
            var selectedItem = this.TreeViewModel.SelectedItem;
            if (selectedItem != null && selectedItem.NodeType == NodeType.File && this.IseIntegrator.OpenFiles.Contains(selectedItem.Path) && !this.IseIntegrator.IsFileSaved(selectedItem.Path))
            {
                this.IseIntegrator.GoToFile(selectedItem.Path);
                this.MessageBoxHelper.ShowInfo("Please save your changes or close the file first.");
                return false;
            }
            return true;
        }
    }
}
