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
    public static class PowershellTokenizerProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly Boolean isPowershell5Available;

        static PowershellTokenizerProvider() {
            Type t = typeof(System.Management.Automation.Language.AstVisitor);
            string assemblyName = t.Assembly.FullName.ToString();
            isPowershell5Available = Type.GetType("System.Management.Automation.Language.AstVisitor2," + assemblyName, false) != null;
        }

        public static IPowershellTokenizer GetPowershellTokenizer()
        {
            if (isPowershell5Available)
            {
                Logger.Info("Using Powershell5Tokenizer");
                return new Powershell5Tokenizer();
            }
            else
            {
                Logger.Info("Using PowershellLegacyTokenizer");
                return new PowershellLegacyTokenizer();
            }
        }
    }
}
