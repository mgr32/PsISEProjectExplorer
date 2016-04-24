using Ookii.Dialogs.Wpf;
using PsISEProjectExplorer.UI.Helpers;
using PsISEProjectExplorer.UI.ViewModel;
using System.IO;

namespace PsISEProjectExplorer.Commands
{
    public class ChangeWorkspaceCommand : Command
    {

        private MainViewModel MainViewModel { get; set; }

        private MessageBoxHelper MessageBoxHelper { get; set; }

        public ChangeWorkspaceCommand(MainViewModel mainViewModel, MessageBoxHelper messageBoxHelper)
        {
            this.MainViewModel = mainViewModel;
            this.MessageBoxHelper = messageBoxHelper;
        }

        public void Execute()
        {
            var dialog = new VistaFolderBrowserDialog
            {
                SelectedPath = this.MainViewModel.WorkspaceDirectoryModel.CurrentWorkspaceDirectory,
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
                    this.MainViewModel.WorkspaceDirectoryModel.SetWorkspaceDirectory(dialog.SelectedPath);
                    this.MainViewModel.WorkspaceDirectoryModel.AutoUpdateRootDirectory = false;
                }
            }
        }
    }
}
