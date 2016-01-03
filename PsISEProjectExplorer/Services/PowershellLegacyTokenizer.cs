using NLog;
using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Text.RegularExpressions;

namespace PsISEProjectExplorer.Services
{
    public class PowershellLegacyTokenizer : PowershellBaseTokenizer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private PowershellLegacyTokenVisitor tokenVisitor;

        public PowershellLegacyTokenizer()
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
