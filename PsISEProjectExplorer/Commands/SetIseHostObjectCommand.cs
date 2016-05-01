using Microsoft.PowerShell.Host.ISE;
using PsISEProjectExplorer.UI.IseIntegration;
using PsISEProjectExplorer.UI.ViewModel;
using System.Collections.Generic;

namespace PsISEProjectExplorer.Commands
{
    [Component]
    public class SetIseHostObjectCommand : ParameterizedCommand<ObjectModelRoot>
    {
        private readonly IseIntegrator iseIntegrator;

        private readonly IseFileReloader iseFileReloader;

        private readonly CommandExecutor commandExecutor;

        private readonly MainViewModel mainViewModel;

        public SetIseHostObjectCommand(IseIntegrator iseIntegrator, IseFileReloader iseFileReloader, MainViewModel mainViewModel, CommandExecutor commandExecutor)
        {
            this.iseIntegrator = iseIntegrator;
            this.iseFileReloader = iseFileReloader;
            this.commandExecutor = commandExecutor;
            this.mainViewModel = mainViewModel;
        }

        public void Execute(ObjectModelRoot objectModelRoot)
        {
            this.iseIntegrator.setHostObject(objectModelRoot);
            this.iseIntegrator.FileTabChanged += ResetWorkspaceOnFileTabChanged;
            this.iseFileReloader.startWatching();
            this.commandExecutor.ExecuteWithParam<ResetWorkspaceDirectoryCommand, bool>(true);
        }

        private void ResetWorkspaceOnFileTabChanged(object sender, IseEventArgs args)
        {
            this.commandExecutor.ExecuteWithParam<ResetWorkspaceDirectoryCommand, bool>(false);
            this.mainViewModel.ActiveDocumentPotentiallyChanged();
        }
    }
}
