using PsISEProjectExplorer.UI.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.Commands
{
    public class FindInFilesCommand : Command
    {
        private MainViewModel MainViewModel { get; set; }

        private ProjectExplorerWindow ProjectExplorerWindow { get; set; }

        public FindInFilesCommand(MainViewModel mainViewModel, ProjectExplorerWindow projectExplorerWindow)
        {
            this.MainViewModel = mainViewModel;
            this.ProjectExplorerWindow = projectExplorerWindow;
        }

        public void Execute()
        {
            this.MainViewModel.SearchOptions.SearchText = string.Empty;
            this.MainViewModel.SearchInFiles = true;

            this.ProjectExplorerWindow.FocusOnTextBoxSearchText();
        }
    }
}
