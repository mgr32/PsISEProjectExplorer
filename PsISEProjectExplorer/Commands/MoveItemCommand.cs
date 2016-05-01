
using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.Services;
using PsISEProjectExplorer.UI.Helpers;
using PsISEProjectExplorer.UI.IseIntegration;
using PsISEProjectExplorer.UI.ViewModel;
using System;

namespace PsISEProjectExplorer.Commands
{
    [Component]
    public class MoveItemCommand : ParameterizedCommand<Tuple<TreeViewEntryItemModel, TreeViewEntryItemModel>>
    {
        private readonly TreeViewModel treeViewModel;

        private readonly MessageBoxHelper messageBoxHelper;

        private readonly WorkspaceDirectoryModel workspaceDirectoryModel;

        private readonly FilesPatternProvider filesPatternProvider;

        private readonly FileSystemOperationsService fileSystemOperationsService;

        private readonly IseIntegrator iseIntegrator;

        private readonly UnsavedFileChecker unsavedFileEnforcer;

        public MoveItemCommand(TreeViewModel treeViewModel, MessageBoxHelper messageBoxHelper, WorkspaceDirectoryModel workspaceDirectoryModel,
            FilesPatternProvider filesPatternProvider, FileSystemOperationsService fileSystemOperationsService, IseIntegrator iseIntegrator,
            UnsavedFileChecker unsavedFileEnforcer)
        {
            this.treeViewModel = treeViewModel;
            this.messageBoxHelper = messageBoxHelper;
            this.workspaceDirectoryModel = workspaceDirectoryModel;
            this.filesPatternProvider = filesPatternProvider;
            this.fileSystemOperationsService = fileSystemOperationsService;
            this.iseIntegrator = iseIntegrator;
            this.unsavedFileEnforcer = unsavedFileEnforcer;
        }
        public void Execute(Tuple<TreeViewEntryItemModel, TreeViewEntryItemModel> param)
        {
            var movedItem = param.Item1;
            var destinationItem = param.Item2;
            if (movedItem == destinationItem || movedItem == null)
            {
                return;
            }
            if (!this.unsavedFileEnforcer.EnsureCurrentlyOpenedFileIsSaved())
            {
                return;
            }
            string rootDirectory = this.workspaceDirectoryModel.CurrentWorkspaceDirectory;
            string destPath = destinationItem != null ? destinationItem.Path : rootDirectory;
            if (!this.messageBoxHelper.ShowConfirmMessage(String.Format("Please confirm you want to move '{0}' to '{1}'.", movedItem.Path, destPath)))
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
                this.filesPatternProvider.RemoveAdditionalPath(movedItem.Path);
                this.filesPatternProvider.AddAdditionalPath(newPath);
                bool closed = this.iseIntegrator.CloseFile(movedItem.Path);
                fileSystemOperationsService.RenameFileOrDirectory(movedItem.Path, newPath);
                if (closed)
                {
                    this.iseIntegrator.GoToFile(newPath);
                }
                if (destinationItem != null)
                {
                    destinationItem.IsExpanded = true;
                }
            }
            catch (Exception e)
            {
                this.treeViewModel.PathOfItemToSelectOnRefresh = null;
                this.messageBoxHelper.ShowError("Failed to move: " + e.Message);
            }
        }

        private string GenerateNewPath(string currentPath, string newValue)
        {
            var newPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(currentPath), newValue);
            this.treeViewModel.PathOfItemToSelectOnRefresh = newPath;
            return newPath;
        }

        private string GenerateNewPathForDir(string currentPath, string newValue)
        {
            var newPath = System.IO.Path.Combine(currentPath, newValue);
            this.treeViewModel.PathOfItemToSelectOnRefresh = newPath;
            return newPath;
        }
    }
}
