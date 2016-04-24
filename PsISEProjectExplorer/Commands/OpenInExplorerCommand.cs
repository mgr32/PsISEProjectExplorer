using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.UI.Helpers;
using PsISEProjectExplorer.UI.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.Commands
{
    public class OpenInExplorerCommand : Command
    {
        private ProjectExplorerWindow ProjectExplorerWindow { get; set; }

        private MessageBoxHelper MessageBoxHelper { get; set; }

        public OpenInExplorerCommand(ProjectExplorerWindow projectExplorerWindow, MessageBoxHelper messageBoxHelper)
        {
            this.ProjectExplorerWindow = projectExplorerWindow;
            this.MessageBoxHelper = messageBoxHelper;
        }

        public void Execute()
        {
            var item = (TreeViewEntryItemModel)this.ProjectExplorerWindow.SearchResultsTreeView.SelectedItem;
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
