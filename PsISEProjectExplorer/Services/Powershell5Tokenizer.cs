using NLog;
using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;

namespace PsISEProjectExplorer.Services
{
    public class Powershell5Tokenizer : PowershellBaseTokenizer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private Powershell5TokenVisitor tokenVisitor;

        public Powershell5Tokenizer(bool dslAutoDiscovery, IEnumerable<string> dslCustomDictionary) : base(dslAutoDiscovery, dslCustomDictionary)
        {
            this.tokenVisitor = new Powershell5TokenVisitor(this.OnFunctionVisit,
                this.OnConfigurationVisit,
                this.OnClassVisit,
                this.OnClassPropertyVisit,
                this.OnClassConstructorVisit,
                this.OnClassMethodVisit,
                this.GetDslInstanceName,
                this.OnDslVisit
                );
        }

        protected override void VisitTokens(Ast ast)
        {
            this.tokenVisitor.VisitTokens(ast);
        }

        private object OnClassVisit(string name, IScriptExtent extent, int nestingLevel, object parent)
        {
            return CreateNewPowershellItem(PowershellItemType.Class, name, extent, (PowershellItem)parent, nestingLevel);
        }

        private object OnClassPropertyVisit(string name, IScriptExtent extent, int nestingLevel, object parent)
        {
            return CreateNewPowershellItem(PowershellItemType.ClassProperty, name, extent, (PowershellItem)parent, nestingLevel);
        }

        private object OnClassConstructorVisit(string name, IScriptExtent extent, int nestingLevel, object parent)
        {
            return CreateNewPowershellItem(PowershellItemType.ClassConstructor, name, extent, (PowershellItem)parent, nestingLevel);
        }

        private object OnClassMethodVisit(string name, IScriptExtent extent, int nestingLevel, object parent)
        {
            return CreateNewPowershellItem(PowershellItemType.ClassMethod, name, extent, (PowershellItem)parent, nestingLevel);
        }


    }
}
