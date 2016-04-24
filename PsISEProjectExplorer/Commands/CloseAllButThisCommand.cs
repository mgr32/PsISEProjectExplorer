using PsISEProjectExplorer.UI.IseIntegration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
