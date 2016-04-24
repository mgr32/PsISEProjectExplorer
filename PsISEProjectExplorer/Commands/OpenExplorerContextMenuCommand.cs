using GongSolutions.Shell;
using NLog;
using PsISEProjectExplorer.UI.ViewModel;
using System;
using System.IO;

namespace PsISEProjectExplorer.Commands
{
    public class OpenExplorerContextMenuCommand : Command
    {

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private TreeViewModel TreeViewModel { get; set; }

        private WorkspaceDirectoryModel WorkspaceDirectoryModel { get; set; }

        public OpenExplorerContextMenuCommand(TreeViewModel treeViewModel, WorkspaceDirectoryModel workspaceDirectoryModel)
        {
            this.TreeViewModel = treeViewModel;
            this.WorkspaceDirectoryModel = workspaceDirectoryModel;
        }

        public void Execute()
        {
            // otherwise, show Windows Explorer context menu
            string path;
            var selectedItem = this.TreeViewModel.SelectedItem;
            if (selectedItem == null)
            {
                path = this.WorkspaceDirectoryModel.CurrentWorkspaceDirectory;
            }
            else
            {
                path = selectedItem.Path;
            }

            if (String.IsNullOrEmpty(path) || (!File.Exists(path) && !Directory.Exists(path)))
            {
                return;
            }

            var uri = new Uri(path);
            ShellItem shellItem = new ShellItem(uri);
            ShellContextMenu menu = new ShellContextMenu(shellItem);
            try
            {
                menu.ShowContextMenu(System.Windows.Forms.Control.MousePosition);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to show Windows Explorer Context Menu");
            }
        }
    }
}
