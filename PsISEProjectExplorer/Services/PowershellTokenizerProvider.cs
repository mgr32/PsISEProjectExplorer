using NLog;
using PsISEProjectExplorer.Config;
using System;

namespace PsISEProjectExplorer.Services
{
    [Component]
    public class PowershellTokenizerProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IPowershellTokenizer powershellTokenizer;

        public IPowershellTokenizer PowershellTokenizer { get { return this.powershellTokenizer; } }

        public PowershellTokenizerProvider(ConfigValues configValues) {
            Type t = typeof(System.Management.Automation.Language.AstVisitor);
            string assemblyName = t.Assembly.FullName.ToString();
            bool isPowershell5Available = Type.GetType("System.Management.Automation.Language.AstVisitor2," + assemblyName, false) != null;
            if (isPowershell5Available)
            {
                Logger.Info("Using Powershell5Tokenizer");
                this.powershellTokenizer = new Powershell5Tokenizer(configValues);
            }
            else
            {
                Logger.Info("Using PowershellLegacyTokenizer");
                this.powershellTokenizer = new PowershellLegacyTokenizer(configValues);
            }
        }
    }
}
