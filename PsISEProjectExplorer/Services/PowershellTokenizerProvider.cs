using NLog;
using PsISEProjectExplorer.Config;
using System;

namespace PsISEProjectExplorer.Services
{
    [Component]
    public class PowershellTokenizerProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly Boolean isPowershell5Available;

        private readonly ConfigHandler configHandler;

        private IPowershellTokenizer powershellTokenizer;

        public IPowershellTokenizer PowershellTokenizer { get { return this.powershellTokenizer; } }

        public PowershellTokenizerProvider(ConfigHandler configHandler) {
            this.configHandler = configHandler;
            Type t = typeof(System.Management.Automation.Language.AstVisitor);
            string assemblyName = t.Assembly.FullName.ToString();
            isPowershell5Available = Type.GetType("System.Management.Automation.Language.AstVisitor2," + assemblyName, false) != null;
            if (this.isPowershell5Available)
            {
                Logger.Info("Using Powershell5Tokenizer");
                this.powershellTokenizer = new Powershell5Tokenizer(this.configHandler);
            }
            else
            {
                Logger.Info("Using PowershellLegacyTokenizer");
                this.powershellTokenizer = new PowershellLegacyTokenizer(this.configHandler);
            }
        }
    }
}
