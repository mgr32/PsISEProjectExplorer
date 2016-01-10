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
        private static readonly Regex ImportDscRegex = new Regex("Import-DSCResource", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private PowershellLegacyTokenVisitor tokenVisitor;

        public PowershellLegacyTokenizer(bool dslAutoDiscovery, IEnumerable<string> dslCustomDictionary) : base(dslAutoDiscovery, dslCustomDictionary)
        {
            this.tokenVisitor = new PowershellLegacyTokenVisitor(
                this.OnFunctionVisit,
                this.OnConfigurationVisit,
                this.GetDslInstanceName,
                this.OnDslVisit);
        }

        public override PowershellItem GetPowershellItems(string path, string contents)
        {
            ParseError[] errors;
            Token[] tokens;
            string newContents;
            AddedItems = new HashSet<string>();

            // this is fix for performance issue in PSParser.Tokenize - when file contains Import-DSCResource pointing to a non-installed resource, 
            // parsing takes long time and 'Unable to load resource' errors appear - note it seems to be fixed in PS5
            bool dscParse = this.RemoveImportDscResource(contents, out newContents);
            Ast ast = Parser.ParseInput(newContents, out tokens, out errors);
            var errorsLog = !errors.Any() || dscParse ? null :
                "Parsing error(s): " + Environment.NewLine + string.Join(Environment.NewLine, errors.OrderBy(err => err.Extent.StartLineNumber).Select(err => "Line " + err.Extent.StartLineNumber + ": " + err.Message));
            RootItem = new PowershellItem(PowershellItemType.Root, null, 0, 0, 0, 0, null, errorsLog);

            VisitTokens(ast);

            return RootItem;
        }

        protected override void VisitTokens(Ast ast)
        {
            this.tokenVisitor.visitTokens(ast);
        }

        protected bool RemoveImportDscResource(string contents, out string newContents)
        {
            if (ImportDscRegex.IsMatch(contents))
            {
                newContents = ImportDscRegex.Replace(contents, "#Import-DSCResource");
                return true;
            }
            newContents = contents;
            return false;
        }

    }
}
