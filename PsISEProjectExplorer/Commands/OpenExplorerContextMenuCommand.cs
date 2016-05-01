using GongSolutions.Shell;
using NLog;
using PsISEProjectExplorer.UI.ViewModel;
using System;
using System.IO;

namespace PsISEProjectExplorer.Commands
{
    [Component]
    public class OpenExplorerContextMenuCommand : Command
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly TreeViewModel treeViewModel;

        private readonly WorkspaceDirectoryModel workspaceDirectoryModel;

        public OpenExplorerContextMenuCommand(TreeViewModel treeViewModel, WorkspaceDirectoryModel workspaceDirectoryModel)
        {
            this.treeViewModel = treeViewModel;
            this.workspaceDirectoryModel = workspaceDirectoryModel;
        }

        public void Execute()
        {
            string path;
            var selectedItem = this.treeViewModel.SelectedItem;
            if (selectedItem == null)
            {
                path = this.workspaceDirectoryModel.CurrentWorkspaceDirectory;
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
