using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.UI.Helpers;
using PsISEProjectExplorer.UI.ViewModel;
using System;
using System.Diagnostics;

namespace PsISEProjectExplorer.Commands
{
    public class OpenInExplorerCommand : Command
    {
        private TreeViewModel TreeViewModel { get; set; }

        private MessageBoxHelper MessageBoxHelper { get; set; }

        public OpenInExplorerCommand(TreeViewModel TreeViewModel, MessageBoxHelper messageBoxHelper)
        {
            this.TreeViewModel = TreeViewModel;
            this.MessageBoxHelper = messageBoxHelper;
        }

        public void Execute()
        {
            var item = this.TreeViewModel.SelectedItem;
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
               this. MessageBoxHelper.ShowError(string.Format("Cannot open path: '{0}' - {1}.", item.Path, ex.Message));
            }
        }
    }
}
