
using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.Services;
using PsISEProjectExplorer.UI.Helpers;
using PsISEProjectExplorer.UI.IseIntegration;
using PsISEProjectExplorer.UI.ViewModel;
using System;

namespace PsISEProjectExplorer.Commands
{
    public class MoveItemCommand : ParameterizedCommand<Tuple<TreeViewEntryItemModel, TreeViewEntryItemModel>>
    {

        private TreeViewModel TreeViewModel { get; set; }

        private MessageBoxHelper MessageBoxHelper { get; set;  }

        private WorkspaceDirectoryModel WorkspaceDirectoryModel { get; set; }

        private FilesPatternProvider FilesPatternProvider { get; set; }

        private FileSystemOperationsService FileSystemOperationsService { get; set; }

        private IseIntegrator IseIntegrator { get; set; }

        private UnsavedFileChecker UnsavedFileEnforcer { get; set; }

        public MoveItemCommand(TreeViewModel treeViewModel, MessageBoxHelper messageBoxHelper, WorkspaceDirectoryModel workspaceDirectoryModel,
            FilesPatternProvider filesPatternProvider, FileSystemOperationsService fileSystemOperationsService, IseIntegrator iseIntegrator,
            UnsavedFileChecker unsavedFileEnforcer)
        {
            this.TreeViewModel = treeViewModel;
            this.MessageBoxHelper = messageBoxHelper;
            this.WorkspaceDirectoryModel = workspaceDirectoryModel;
            this.FilesPatternProvider = filesPatternProvider;
            this.FileSystemOperationsService = fileSystemOperationsService;
            this.IseIntegrator = iseIntegrator;
            this.UnsavedFileEnforcer = unsavedFileEnforcer;
        }
        public void Execute(Tuple<TreeViewEntryItemModel, TreeViewEntryItemModel> param)
        {
            var movedItem = param.Item1;
            var destinationItem = param.Item2;
            if (movedItem == destinationItem || movedItem == null)
            {
                return;
            }
            if (!this.UnsavedFileEnforcer.EnsureCurrentlyOpenedFileIsSaved())
            {
                return;
            }
            string rootDirectory = this.WorkspaceDirectoryModel.CurrentWorkspaceDirectory;
            string destPath = destinationItem != null ? destinationItem.Path : rootDirectory;
            if (!this.MessageBoxHelper.ShowConfirmMessage(String.Format("Please confirm you want to move '{0}' to '{1}'.", movedItem.Path, destPath)))
            {
                return;
            }
            try
            {
                string newPath;
                // moved to the empty place, i.e. to the workspace directory
                if (destinationItem == null)
                {
                    newPath = this.GenerateNewPathForDir(rootDirectory, movedItem.Name);
                }
                else if (destinationItem.NodeType == NodeType.File)
                {
                    newPath = this.GenerateNewPath(destinationItem.Path, movedItem.Name);
                }
                else if (destinationItem.NodeType == NodeType.Directory)
                {
                    newPath = this.GenerateNewPathForDir(destinationItem.Path, movedItem.Name);
                }
                else
                {
                    return;
                }
                this.FilesPatternProvider.RemoveAdditionalPath(movedItem.Path);
                this.FilesPatternProvider.AddAdditionalPath(newPath);
                bool closed = this.IseIntegrator.CloseFile(movedItem.Path);
                FileSystemOperationsService.RenameFileOrDirectory(movedItem.Path, newPath);
                if (closed)
                {
                    this.IseIntegrator.GoToFile(newPath);
                }
                if (destinationItem != null)
                {
                    destinationItem.IsExpanded = true;
                }
            }
            catch (Exception e)
            {
                this.TreeViewModel.PathOfItemToSelectOnRefresh = null;
                this.MessageBoxHelper.ShowError("Failed to move: " + e.Message);
            }
        }

        private string GenerateNewPath(string currentPath, string newValue)
        {
            var newPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(currentPath), newValue);
            this.TreeViewModel.PathOfItemToSelectOnRefresh = newPath;
            return newPath;
        }

        private string GenerateNewPathForDir(string currentPath, string newValue)
        {
            var newPath = System.IO.Path.Combine(currentPath, newValue);
            this.TreeViewModel.PathOfItemToSelectOnRefresh = newPath;
            return newPath;
        }
    }
}
