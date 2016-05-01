using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.UI.Helpers;
using PsISEProjectExplorer.UI.ViewModel;
using System;
using System.Diagnostics;

namespace PsISEProjectExplorer.Commands
{
    [Component]
    public class OpenInExplorerCommand : Command
    {
        private readonly TreeViewModel treeViewModel;

        private readonly MessageBoxHelper messageBoxHelper;

        public OpenInExplorerCommand(TreeViewModel TreeViewModel, MessageBoxHelper messageBoxHelper)
        {
            this.treeViewModel = TreeViewModel;
            this.messageBoxHelper = messageBoxHelper;
        }

        public void Execute()
        {
            var item = this.treeViewModel.SelectedItem;
            if (item == null)
            {
                return;
            }

            try
            {
                switch (item.NodeType)
                {
                    case NodeType.Directory:
                        Process.Start(item.Path);
                        break;
                    case NodeType.File:
                        Process.Start("explorer.exe", "/select, \"" + item.Path + "\"");
                        break;
                }

            }
            catch (Exception ex)
            {
               this. messageBoxHelper.ShowError(string.Format("Cannot open path: '{0}' - {1}.", item.Path, ex.Message));
            }
        }
    }
}
