using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;

namespace PsISEProjectExplorer.Services
{
    public static class PowershellTokenizer
    {
        public static PowershellItem GetPowershellItems(string contents)
        {
            Collection<PSParseError> errors;
            IEnumerable<PSToken> tokens = PSParser.Tokenize(contents, out errors);
            PowershellItem rootItem = new PowershellItem(PowershellItemType.Root, null, 0, 0, 0, null);   
            PowershellItem currentItem = rootItem;

            bool nextTokenIsFunctionName = false;
            PSToken commandToken = null;

            int nestingLevel = 0;
            foreach (PSToken token in tokens)
            {
                if (nextTokenIsFunctionName)
                {
                    var item = new PowershellItem(PowershellItemType.Function, token.Content, token.StartLine, token.StartColumn, nestingLevel, currentItem);
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
                    if (tokenContent == "function" || tokenContent == "filter")
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
