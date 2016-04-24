using PsISEProjectExplorer.Config;
using System.Windows;

namespace PsISEProjectExplorer.Commands
{
    [Component]
    public class CommandExecutor
    {

        private BootstrapConfig BootstrapConfig { get; set; }

        public CommandExecutor(BootstrapConfig bootstrapConfig)
        {
            this.BootstrapConfig = bootstrapConfig;
        }

        public void Execute<T>() where T : class, Command
        {
            var command = this.BootstrapConfig.GetInstance<T>();
            Application.Current.Dispatcher.Invoke(() => command.Execute());
        }

        public void ExecuteWithParam<T, P>(P param) where T : class, ParameterizedCommand<P>
        {
            var command = this.BootstrapConfig.GetInstance<T>();
            Application.Current.Dispatcher.Invoke(() => command.Execute(param));
        }
    }
}
