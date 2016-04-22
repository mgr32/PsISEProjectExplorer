using NLog;
using PsISEProjectExplorer.Config;
using System.Management.Automation.Language;

namespace PsISEProjectExplorer.Services
{
    public class PowershellLegacyTokenizer : PowershellBaseTokenizer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private PowershellLegacyTokenVisitor tokenVisitor;

        public PowershellLegacyTokenizer(ConfigHandler configHandler) : base(configHandler)
        {
            this.tokenVisitor = new PowershellLegacyTokenVisitor(
                this.OnFunctionVisit,
                this.OnConfigurationVisit,
                this.GetDslInstanceName,
                this.OnDslVisit);
        }

        protected override void VisitTokens(Ast ast)
        {
            this.tokenVisitor.visitTokens(ast);
        }

    }
}
