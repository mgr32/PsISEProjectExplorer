using PsISEProjectExplorer.UI.IseIntegration;

namespace PsISEProjectExplorer.Commands
{
    public class CloseAllButThisCommand : Command
    {

        private IseIntegrator IseIntegrator { get; set; }

        public CloseAllButThisCommand(IseIntegrator iseIntegrator)
        {
            this.IseIntegrator = IseIntegrator;
        }

        public void Execute()
        {
            this.IseIntegrator.CloseAllButThis();
        }
    }
}
