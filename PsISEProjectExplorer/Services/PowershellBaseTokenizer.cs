using PsISEProjectExplorer.Config;
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
    public abstract class PowershellBaseTokenizer : IPowershellTokenizer
    {
        private static readonly Regex ImportDscRegex = new Regex("Import-DSCResource", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        protected ISet<string> AddedItems;

        protected PowershellItem RootItem;

        private bool dslAutoDiscovery;

        private IEnumerable<string> dslCustomDictionary;

        private bool parsePowershellDscWithExternalImports;

        private readonly ConfigValues configValues;

        public PowershellBaseTokenizer(ConfigValues configValues)
        {
            this.configValues = configValues;
            this.dslAutoDiscovery = configValues.DslAutoDiscovery;
            this.dslCustomDictionary = configValues.DslCustomDictionary;
            // this is fix for performance issue in PSParser.Tokenize - when file contains Import - DSCResource pointing to a non-installed resource,
            // parsing takes long time and 'Unable to load resource' errors appear 
            this.parsePowershellDscWithExternalImports = configValues.ParsePowershellDSCWithExternalImports;
        }

        public PowershellItem GetPowershellItems(string path, string contents)
        {
            bool parsePowershellItems = configValues.ParsePowershellItems;
            if (!parsePowershellItems || !this.parsePowershellDscWithExternalImports && ImportDscRegex.IsMatch(contents))
            {
                return this.createRootItem(null);
            }

            ParseError[] errors;
            Token[] tokens;
            AddedItems = new HashSet<string>();
            
            
            Ast ast = Parser.ParseInput(contents, out tokens, out errors);
            var errorsLog = !errors.Any() ? null :
                "Parsing error(s): " + Environment.NewLine + string.Join(Environment.NewLine, errors.OrderBy(err => err.Extent.StartLineNumber).Select(err => "Line " + err.Extent.StartLineNumber + ": " + err.Message));
            RootItem = this.createRootItem(errorsLog);

            VisitTokens(ast);

            return RootItem;
        }

        private PowershellItem createRootItem(string errorsLog)
        {
            RootItem = new PowershellItem(PowershellItemType.Root, null, 0, 0, 0, 0, null, errorsLog);
            return RootItem;
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

        public string GetTokenAtColumn(string line, int column)
        {
            Collection<PSParseError> errors;
            IEnumerable<PSToken> tokens = PSParser.Tokenize(line, out errors);
            return tokens.Where(token => token.StartColumn <= column && token.EndColumn >= column).Select(token => token.Content).FirstOrDefault();
        }

        protected PowershellItem CreateNewPowershellItem(PowershellItemType type, string itemName, int startColumnNumber, int endColumnNumber, int startLineNumber, PowershellItem parent, int nestingLevel)
        {
            string itemKey = startLineNumber + "_" + startColumnNumber;
            if (!AddedItems.Contains(itemKey))
            {
                AddedItems.Add(itemKey);
                if (parent == null)
                {
                    parent = RootItem;
                }
                return new PowershellItem(type, itemName, startLineNumber, startColumnNumber, endColumnNumber, nestingLevel, parent, null);
            }
            else
            {
                return null;
            }
        }

        protected PowershellItem CreateNewPowershellItem(PowershellItemType type, string itemName, IScriptExtent extent, PowershellItem parent, int nestingLevel)
        {
            int startColumnNumber = extent.StartColumnNumber + extent.Text.IndexOf(itemName);
            int endColumnNumber = startColumnNumber + itemName.Length;
            int startLineNumber = extent.StartLineNumber;
            return CreateNewPowershellItem(type, itemName, startColumnNumber, endColumnNumber, startLineNumber, parent, nestingLevel);
        }

        protected object OnFunctionVisit(string name, IScriptExtent extent, int nestingLevel, object parent)
        {
            return CreateNewPowershellItem(PowershellItemType.Function, name, extent, (PowershellItem)parent, nestingLevel);
        }

        protected object OnConfigurationVisit(string name, IScriptExtent extent, int nestingLevel, object parent)
        {
            int startColumnNumber = extent.StartColumnNumber + "configuration".Length + 1;
            int endColumnNumber = startColumnNumber + name.Length;
            int startLineNumber = extent.StartLineNumber;
            return CreateNewPowershellItem(PowershellItemType.Configuration, name, startColumnNumber, endColumnNumber, startLineNumber, (PowershellItem)parent, nestingLevel);
        }

        protected object OnDslVisit(string dslTypeName, string dslInstanceName, IScriptExtent extent, int nestingLevel, object parent)
        {
            string name = dslTypeName + " " + dslInstanceName;
            int endColumnNumber = extent.StartColumnNumber + dslTypeName.Length;
            return CreateNewPowershellItem(PowershellItemType.DslElement, name, extent.StartColumnNumber, endColumnNumber, extent.StartLineNumber, (PowershellItem)parent, nestingLevel);
        }

        protected string GetDslInstanceName(ReadOnlyCollection<CommandElementAst> commandElements, Ast parent)
        {
            // todo: what with "configuration" in legacy mode?
            if (commandElements == null || commandElements.Count < 2)
            {
                return null;
            }


            // in order to be possibly a DSL expression, first element must be StringConstant AND second must not be =
            // AND (first element must start with PSDesiredStateConfiguration - legacy OR (last must be ScriptBlockExpression, and last but 1 must not be CommandParameter))
            if (!(commandElements[0] is StringConstantExpressionAst) ||
                ((commandElements[1] is StringConstantExpressionAst && ((StringConstantExpressionAst)commandElements[1]).Value == "=")))
            {
                return null;
            }

            string dslTypeName = ((StringConstantExpressionAst)commandElements[0]).Value;
            if (!dslTypeName.StartsWith("PSDesiredStateConfiguration") &&
                !this.dslCustomDictionary.Contains(dslTypeName.ToLowerInvariant()) &&
                (!this.dslAutoDiscovery || (!(commandElements[commandElements.Count - 1] is ScriptBlockExpressionAst || commandElements[commandElements.Count - 1] is HashtableAst) ||
                commandElements[commandElements.Count - 2] is CommandParameterAst)))
            {
                return null;
            }

            // additionally, parent must not be a Pipeline that has more than 1 element 
            if (parent is PipelineAst && ((PipelineAst)parent).PipelineElements.Count > 1)
            {
                return null;
            }

            return this.TrimQuotes(this.GetDslInstanceName(dslTypeName, commandElements));
        }

        private string GetDslInstanceName(string dslTypeName, IEnumerable<CommandElementAst> commandElements)
        {
            // try to guess dsl instance name - first string constant that is not named parameter value (or is value of 'name' parameter)
            bool lastElementIsUnknownParameter = false;
            int num = 0;

            foreach (CommandElementAst elementAst in commandElements)
            {
                if (num++ == 0)
                {
                    continue;
                }
                if (elementAst is CommandParameterAst)
                {
                    CommandParameterAst commandParameterAst = (CommandParameterAst)elementAst;
                    lastElementIsUnknownParameter = commandParameterAst.ParameterName.ToLowerInvariant() != "name";
                    if (dslTypeName.StartsWith("PSDesiredStateConfiguration") && !lastElementIsUnknownParameter)
                    {
                        return commandParameterAst.Argument.ToString();
                    }
                    continue;
                }
                if (elementAst is StringConstantExpressionAst && !lastElementIsUnknownParameter)
                {
                    return ((StringConstantExpressionAst)elementAst).Value;
                }
                if (elementAst is ExpandableStringExpressionAst && !lastElementIsUnknownParameter)
                {
                    return ((ExpandableStringExpressionAst)elementAst).Value;
                }
                if (elementAst is MemberExpressionAst && !lastElementIsUnknownParameter)
                {
                    return ((MemberExpressionAst)elementAst).Extent.Text;
                }

                lastElementIsUnknownParameter = false;
            }
            return string.Empty;
        }

        private string TrimQuotes(string str)
        {
            return Regex.Replace(str, "('|\")(.+)('|\")", "$2");
        }


        protected abstract void VisitTokens(Ast ast);

    }
}
