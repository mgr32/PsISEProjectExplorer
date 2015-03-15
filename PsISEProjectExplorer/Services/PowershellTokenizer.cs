using NLog;
using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;

namespace PsISEProjectExplorer.Services
{
    public static class PowershellTokenizer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly Regex importDscRegex = new Regex("Import-DSCResource", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static PowershellItem GetPowershellItems(string path, string contents)
        {
            
            Collection<PSParseError> errors;
            bool dscParse = false;
            // this is fix for performance issue in PSParser.Tokenize - when file contains Import-DSCResource pointing to a non-installed resource, 
            // parsing takes long time and 'Unable to load resource' errors appear
            if (importDscRegex.IsMatch(contents))
            {
                contents = importDscRegex.Replace(contents, "#Import-DSCResource");
                dscParse = true;
            }
            IEnumerable<PSToken> tokens = PSParser.Tokenize(contents, out errors);
            var errorsLog = !errors.Any() || dscParse ? null :
                "Parsing error(s): " + Environment.NewLine + string.Join(Environment.NewLine, errors.OrderBy(err => err.Token.StartLine).Select(err => "Line " + err.Token.StartLine + ": " + err.Message));
            PowershellItem rootItem = new PowershellItem(PowershellItemType.Root, null, 0, 0, 0, null, errorsLog);
            if (errorsLog != null) 
            {
                Logger.Debug("File " + path + " - " + errorsLog);
            }
            PowershellItem currentItem = rootItem;

            bool nextTokenIsFunctionName = false;
            PSToken commandToken = null;

            int nestingLevel = 0;
            foreach (PSToken token in tokens)
            {
                if (nextTokenIsFunctionName)
                {
                    var item = new PowershellItem(PowershellItemType.Function, token.Content, token.StartLine, token.StartColumn, nestingLevel, currentItem, null);
                    // currentItem = item;
                    nextTokenIsFunctionName = false;
                }
                /*else if (commandToken != null)
                {
                    var item = new PowershellItem(PowershellItemType.Command, commandToken.Content, commandToken.StartLine, commandToken.StartColumn, nestingLevel, currentItem);
                    currentItem = item;
                    commandToken = null;
                    continue;
                }
                else if (token.Type == PSTokenType.NewLine)
                {
                    if (currentItem != null && nestingLevel <= currentItem.NestingLevel)
                    {
                        currentItem = currentItem.Parent ?? rootItem;
                    }
                }
                else if (token.Type == PSTokenType.GroupStart && token.Content.Contains("{"))
                {
                    nestingLevel++;
                }
                else if (token.Type == PSTokenType.GroupEnd && token.Content.Contains("}"))
                {
                    nestingLevel--;
                }
                else if (token.Type == PSTokenType.Command)
                {
                    commandToken = token;                   
                }*/
                else if (token.Type == PSTokenType.Keyword)
                {
                    string tokenContent = token.Content.ToLowerInvariant();
                    if (tokenContent == "function" || tokenContent == "filter" || tokenContent == "configuration")
                    {
                        nextTokenIsFunctionName = true;
                    }
                }
            }
            return rootItem;
        }

        public static string GetTokenAtColumn(string line, int column)
        {
            Collection<PSParseError> errors;
            IEnumerable<PSToken> tokens = PSParser.Tokenize(line, out errors);
            return tokens.Where(token => token.StartColumn <= column && token.EndColumn >= column).Select(token => token.Content).FirstOrDefault();
        }



    }
}
