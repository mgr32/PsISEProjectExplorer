using PsISEProjectExplorer.Services;
using PsISEProjectExplorer.UI.Helpers;
using PsISEProjectExplorer.UI.IseIntegration;
using PsISEProjectExplorer.UI.ViewModel;
using System;
using System.IO;
using System.Linq;

namespace PsISEProjectExplorer.Commands
{
    public class DeleteItemCommand : Command
    {
        private TreeViewModel TreeViewModel { get; set; }

        private MessageBoxHelper MessageBoxHelper { get; set; }

        private IseIntegrator IseIntegrator { get; set; }

        private FilesPatternProvider FilesPatternProvider { get; set; }

        private FileSystemOperationsService FileSystemOperationsService { get; set; }

        private UnsavedFileChecker UnsavedFileEnforcer { get; set; }

        public DeleteItemCommand(TreeViewModel treeViewModel, MessageBoxHelper messageBoxHelper, IseIntegrator iseIntegrator, 
            FilesPatternProvider filesPatternProvider, FileSystemOperationsService fileSystemOperationsService, UnsavedFileChecker unsavedFileEnforcer)
        {
            this.TreeViewModel = treeViewModel;
            this.MessageBoxHelper = messageBoxHelper;
            this.IseIntegrator = iseIntegrator;
            this.FilesPatternProvider = filesPatternProvider;
            this.FileSystemOperationsService = fileSystemOperationsService;
            this.UnsavedFileEnforcer = unsavedFileEnforcer;
        }

        public void Execute()
        {
            var selectedItem = this.TreeViewModel.SelectedItem;
            if (selectedItem == null)
            {
                return;
            }

            if (!this.UnsavedFileEnforcer.EnsureCurrentlyOpenedFileIsSaved())
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
            if (this.MessageBoxHelper.ShowConfirmMessage(message))
            {
                try
                {
                    this.IseIntegrator.CloseFile(selectedItem.Path);
                    this.FilesPatternProvider.RemoveAdditionalPath(selectedItem.Path);
                    FileSystemOperationsService.DeleteFileOrDirectory(selectedItem.Path);
                }
                catch (Exception e)
                {
                    this.MessageBoxHelper.ShowError("Failed to delete: " + e.Message);
                }
            }
        }

    }
}
