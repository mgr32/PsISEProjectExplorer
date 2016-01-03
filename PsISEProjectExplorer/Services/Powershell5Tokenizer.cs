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

        private ISet<string> addedItems;

        private PowershellItem rootItem;

        public Powershell5Tokenizer()
        {
            this.tokenVisitor = new Powershell5TokenVisitor(this.onFunctionVisit,
                this.onConfigurationVisit,
                this.onClassVisit,
                this.onClassPropertyVisit,
                this.onClassConstructorVisit,
                this.onClassMethodVisit,
                this.onDslVisit
                );
        }

        public override PowershellItem GetPowershellItems(string path, string contents)
        {
            ParseError[] errors;
            Token[] tokens;
            string newContents;
            addedItems = new HashSet<string>();

            bool dscParse = removeImportDscResource(contents, out newContents);
            ScriptBlockAst ast = Parser.ParseInput(newContents, out tokens, out errors);
            var errorsLog = !errors.Any() || dscParse ? null :
                "Parsing error(s): " + Environment.NewLine + string.Join(Environment.NewLine, errors.OrderBy(err => err.Extent.StartLineNumber).Select(err => "Line " + err.Extent.StartLineNumber + ": " + err.Message));
            rootItem = new PowershellItem(PowershellItemType.Root, null, 0, 0, 0, 0, null, errorsLog);

            ast.Visit(tokenVisitor);

            return rootItem;
        }


        private PowershellItem CreateNewPowershellItem(PowershellItemType type, string itemName, IScriptExtent extent, PowershellItem parent, int nestingLevel)
        {
            int startColumnNumber = extent.StartColumnNumber + extent.Text.IndexOf(itemName);
            int endColumnNumber = startColumnNumber + itemName.Length;
            return CreateNewPowershellItem(type, itemName, startColumnNumber, endColumnNumber, extent, parent, nestingLevel);
        }

        private PowershellItem CreateNewPowershellItem(PowershellItemType type, string itemName, int startColumnNumber, int endColumnNumber, IScriptExtent extent, PowershellItem parent, int nestingLevel)
        {
            string itemKey = extent.StartLineNumber + "_" + startColumnNumber;
            if (!addedItems.Contains(itemKey))
            {
                addedItems.Add(itemKey);
                if (parent == null)
                {
                    parent = rootItem;
                }
                return new PowershellItem(type, itemName, extent.StartLineNumber, startColumnNumber, endColumnNumber, nestingLevel, parent, null);
            }
            else
            {
                return null;
            }
        }

        private void onFunctionVisit(string name, IScriptExtent extent)
        {
            CreateNewPowershellItem(PowershellItemType.Function, name, extent, null, 0);
        }

        private void onConfigurationVisit(string name, IScriptExtent extent)
        {
            CreateNewPowershellItem(PowershellItemType.Configuration, name, extent, null, 0);
        }

        private object onClassVisit(string name, IScriptExtent extent)
        {
            return CreateNewPowershellItem(PowershellItemType.Class, name, extent, null, 0);
        }

        private void onClassPropertyVisit(string name, IScriptExtent extent, object classItem)
        {
            CreateNewPowershellItem(PowershellItemType.ClassProperty, name, extent, (PowershellItem)classItem, 1);
        }

        private void onClassConstructorVisit(string name, IScriptExtent extent, object classItem)
        {
            CreateNewPowershellItem(PowershellItemType.ClassConstructor, name, extent, (PowershellItem)classItem, 1);
        }

        private void onClassMethodVisit(string name, IScriptExtent extent, object classItem)
        {
            CreateNewPowershellItem(PowershellItemType.ClassMethod, name, extent, (PowershellItem)classItem, 1);
        }

        private object onDslVisit(string dslTypeName, string dslInstanceName, int nestingLevel, IScriptExtent extent, object parent)
        {
            string name = dslTypeName + " " + dslInstanceName;
            int endColumnNumber = extent.StartColumnNumber + dslTypeName.Length;
            return CreateNewPowershellItem(PowershellItemType.DslElement, name, extent.StartColumnNumber, endColumnNumber, extent, (PowershellItem)parent, nestingLevel);
        }
    }
}
