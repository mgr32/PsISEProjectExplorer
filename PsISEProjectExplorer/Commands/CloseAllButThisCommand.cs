using PsISEProjectExplorer.UI.IseIntegration;

namespace PsISEProjectExplorer.Commands
{
    [Component]
    public class CloseAllButThisCommand : Command
    {
        private readonly IseIntegrator iseIntegrator;

        public CloseAllButThisCommand(IseIntegrator iseIntegrator)
        {
            this.iseIntegrator = iseIntegrator;
        }

        public void Execute()
        {
            this.iseIntegrator.CloseAllButThis();
        }
    }
}
