using PsISEProjectExplorer.Services;
using PsISEProjectExplorer.UI.IseIntegration;
using PsISEProjectExplorer.UI.ViewModel;
using System;
using System.IO;
using System.Windows;

namespace PsISEProjectExplorer.Commands
{
    [Component]
    public class ResetWorkspaceDirectoryCommand : ParameterizedCommand<bool>
    {
        private readonly IseIntegrator iseIntegrator;

        private readonly WorkspaceDirectoryModel workspaceDirectoryModel;

        private readonly RootDirectoryProvider rootDirectoryProvider;

        private readonly FileSystemOperationsService fileSystemOperationsService;

        public ResetWorkspaceDirectoryCommand(IseIntegrator iseIntegrator, WorkspaceDirectoryModel workspaceDirectoryModel, RootDirectoryProvider rootDirectoryProvider,
            FileSystemOperationsService fileSystemOperationsService)
        {
            this.iseIntegrator = iseIntegrator;
            this.workspaceDirectoryModel = workspaceDirectoryModel;
            this.rootDirectoryProvider = rootDirectoryProvider;
            this.fileSystemOperationsService = fileSystemOperationsService;
        }

        public void Execute(bool forceReindex)
        {
            bool changed = this.AutoChangeWorkspaceDirectory();
            if (!changed && forceReindex)
            {
                this.workspaceDirectoryModel.TriggerWorkspaceDirectoryChange();
            }
        }

        private bool AutoChangeWorkspaceDirectory()
        {
            var currentPath = this.iseIntegrator.SelectedFilePath;
            if (String.IsNullOrEmpty(currentPath) || currentPath == this.workspaceDirectoryModel.CurrentWorkspaceDirectory)
            {
                return false;
            }
            if (!this.workspaceDirectoryModel.AutoUpdateRootDirectory && this.workspaceDirectoryModel.CurrentWorkspaceDirectory != null)
            {
                return false;
            }
            string newRootDirectoryToSearch = this.rootDirectoryProvider.GetRootDirectoryToSearch(currentPath);
            if (newRootDirectoryToSearch == null || newRootDirectoryToSearch == this.workspaceDirectoryModel.CurrentWorkspaceDirectory ||
                fileSystemOperationsService.IsSubdirectory(this.workspaceDirectoryModel.CurrentWorkspaceDirectory, newRootDirectoryToSearch) ||
                !Directory.Exists(newRootDirectoryToSearch))
            {
                return false;
            }
            this.workspaceDirectoryModel.SetWorkspaceDirectory(newRootDirectoryToSearch);
            return true;
        }

    }
}
