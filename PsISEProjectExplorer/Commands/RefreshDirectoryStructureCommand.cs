using PsISEProjectExplorer.UI.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.Commands
{
    public class RefreshDirectoryStructureCommand : Command
    {
        private MainViewModel MainViewModel { get; set; }

        public RefreshDirectoryStructureCommand(MainViewModel mainViewModel)
        {
            this.MainViewModel = mainViewModel;
        }

        public void Execute()
        {
            this.MainViewModel.ReindexSearchTree();
        }
    }
}
