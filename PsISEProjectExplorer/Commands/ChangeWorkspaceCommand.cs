using Ookii.Dialogs.Wpf;
using PsISEProjectExplorer.UI.Helpers;
using PsISEProjectExplorer.UI.ViewModel;
using System.IO;

namespace PsISEProjectExplorer.Commands
{
    [Component]
    public class ChangeWorkspaceCommand : Command
    {
        private readonly WorkspaceDirectoryModel workspaceDirectoryModel;

        private readonly MessageBoxHelper messageBoxHelper;

        public ChangeWorkspaceCommand(WorkspaceDirectoryModel workspaceDirectoryModel, MessageBoxHelper messageBoxHelper)
        {
            this.workspaceDirectoryModel = workspaceDirectoryModel;
            this.messageBoxHelper = messageBoxHelper;
        }

        public void Execute()
        {
            var dialog = new VistaFolderBrowserDialog
            {
                SelectedPath = this.workspaceDirectoryModel.CurrentWorkspaceDirectory,
                Description = "Please select the new workspace folder.",
                UseDescriptionForTitle = true
            };

            bool? dialogResult = dialog.ShowDialog();
            if (dialogResult != null && dialogResult.Value)
            {
                if (dialog.SelectedPath == Path.GetPathRoot(dialog.SelectedPath))
                {
                    this.messageBoxHelper.ShowError("Cannot use root directory ('" + dialog.SelectedPath + "'). Please select another path.");
                }
                else
                {
                    this.workspaceDirectoryModel.SetWorkspaceDirectory(dialog.SelectedPath);
                    this.workspaceDirectoryModel.AutoUpdateRootDirectory = false;
                }
            }
        }
    }
}
