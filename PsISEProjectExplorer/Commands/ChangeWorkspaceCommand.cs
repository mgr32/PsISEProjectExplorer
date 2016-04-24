using Ookii.Dialogs.Wpf;
using PsISEProjectExplorer.UI.Helpers;
using PsISEProjectExplorer.UI.ViewModel;
using System.IO;

namespace PsISEProjectExplorer.Commands
{
    public class ChangeWorkspaceCommand : Command
    {

        private WorkspaceDirectoryModel WorkspaceDirectoryModel { get; set; }

        private MessageBoxHelper MessageBoxHelper { get; set; }

        public ChangeWorkspaceCommand(WorkspaceDirectoryModel workspaceDirectoryModel, MessageBoxHelper messageBoxHelper)
        {
            this.WorkspaceDirectoryModel = workspaceDirectoryModel;
            this.MessageBoxHelper = messageBoxHelper;
        }

        public void Execute()
        {
            var dialog = new VistaFolderBrowserDialog
            {
                SelectedPath = this.WorkspaceDirectoryModel.CurrentWorkspaceDirectory,
                Description = "Please select the new workspace folder.",
                UseDescriptionForTitle = true
            };

            bool? dialogResult = dialog.ShowDialog();
            if (dialogResult != null && dialogResult.Value)
            {
                if (dialog.SelectedPath == Path.GetPathRoot(dialog.SelectedPath))
                {
                    this.MessageBoxHelper.ShowError("Cannot use root directory ('" + dialog.SelectedPath + "'). Please select another path.");
                }
                else
                {
                    this.WorkspaceDirectoryModel.SetWorkspaceDirectory(dialog.SelectedPath);
                    this.WorkspaceDirectoryModel.AutoUpdateRootDirectory = false;
                }
            }
        }
    }
}
