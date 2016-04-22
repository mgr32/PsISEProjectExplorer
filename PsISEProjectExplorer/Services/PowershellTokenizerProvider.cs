using NLog;
using PsISEProjectExplorer.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.Services
{
    [Component]
    public class PowershellTokenizerProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly Boolean isPowershell5Available;

        private readonly ConfigHandler configHandler;

        public PowershellTokenizerProvider(ConfigHandler configHandler) {
            this.configHandler = configHandler;
            Type t = typeof(System.Management.Automation.Language.AstVisitor);
            string assemblyName = t.Assembly.FullName.ToString();
            isPowershell5Available = Type.GetType("System.Management.Automation.Language.AstVisitor2," + assemblyName, false) != null;
        }

        public IPowershellTokenizer GetPowershellTokenizer()
        {
            if (this.isPowershell5Available)
            {
                Logger.Info("Using Powershell5Tokenizer");
                return new Powershell5Tokenizer(this.configHandler);
            }
            else
            {
                Logger.Info("Using PowershellLegacyTokenizer");
                return new PowershellLegacyTokenizer(this.configHandler);
            }
        }
    }
}
