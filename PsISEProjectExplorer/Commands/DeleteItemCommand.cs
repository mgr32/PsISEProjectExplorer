using PsISEProjectExplorer.Services;
using PsISEProjectExplorer.UI.Helpers;
using PsISEProjectExplorer.UI.IseIntegration;
using PsISEProjectExplorer.UI.ViewModel;
using System;
using System.IO;
using System.Linq;

namespace PsISEProjectExplorer.Commands
{
    [Component]
    public class DeleteItemCommand : Command
    {
        private readonly TreeViewModel treeViewModel;

        private readonly MessageBoxHelper messageBoxHelper;

        private readonly IseIntegrator iseIntegrator;

        private readonly FilesPatternProvider filesPatternProvider;

        private readonly FileSystemOperationsService fileSystemOperationsService;

        private readonly UnsavedFileChecker unsavedFileChecker;

        public DeleteItemCommand(TreeViewModel treeViewModel, MessageBoxHelper messageBoxHelper, IseIntegrator iseIntegrator, 
            FilesPatternProvider filesPatternProvider, FileSystemOperationsService fileSystemOperationsService, UnsavedFileChecker unsavedFileChecker)
        {
            this.treeViewModel = treeViewModel;
            this.messageBoxHelper = messageBoxHelper;
            this.iseIntegrator = iseIntegrator;
            this.filesPatternProvider = filesPatternProvider;
            this.fileSystemOperationsService = fileSystemOperationsService;
            this.unsavedFileChecker = unsavedFileChecker;
        }

        public void Execute()
        {
            var selectedItem = this.treeViewModel.SelectedItem;
            if (selectedItem == null)
            {
                return;
            }

            if (!this.unsavedFileChecker.EnsureCurrentlyOpenedFileIsSaved())
            {
                return;
            }
            int numFilesInside = 0;
            try
            {
                numFilesInside = Directory.GetFileSystemEntries(selectedItem.Path).Count();
            }
            catch (Exception)
            {
                // ignore - this only has impact on message
            }
            string message = numFilesInside == 0 ?
                String.Format("'{0}' will be deleted permanently.", selectedItem.Path) :
                String.Format("'{0}' will be deleted permanently (together with {1} items inside).", selectedItem.Path, numFilesInside);
            if (this.messageBoxHelper.ShowConfirmMessage(message))
            {
                try
                {
                    this.iseIntegrator.CloseFile(selectedItem.Path);
                    this.filesPatternProvider.RemoveAdditionalPath(selectedItem.Path);
                    fileSystemOperationsService.DeleteFileOrDirectory(selectedItem.Path);
                }
                catch (Exception e)
                {
                    this.messageBoxHelper.ShowError("Failed to delete: " + e.Message);
                }
            }
        }

    }
}
