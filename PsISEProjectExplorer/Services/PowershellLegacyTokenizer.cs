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
    public class PowershellLegacyTokenizer : PowershellBaseTokenizer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public override PowershellItem GetPowershellItems(string path, string contents)
        {
            Collection<PSParseError> errors;
            string newContents;
            bool dscParse = removeImportDscResource(contents, out newContents);
            
            IEnumerable<PSToken> tokens = PSParser.Tokenize(newContents, out errors);
            var errorsLog = !errors.Any() || dscParse ? null :
                "Parsing error(s): " + Environment.NewLine + string.Join(Environment.NewLine, errors.OrderBy(err => err.Token.StartLine).Select(err => "Line " + err.Token.StartLine + ": " + err.Message));
            PowershellItem rootItem = new PowershellItem(PowershellItemType.Root, null, 0, 0, 0, 0, null, errorsLog);

            PowershellItem currentItem = rootItem;

            bool nextTokenIsFunctionName = false;
            foreach (PSToken token in tokens)
            {
                if (nextTokenIsFunctionName)
                {
                    var item = new PowershellItem(PowershellItemType.Function, token.Content, token.StartLine, token.StartColumn, 
                        token.EndColumn, 0, rootItem, null);
                    nextTokenIsFunctionName = false;
                }
                else if (token.Type == PSTokenType.Keyword)
                {
                    string tokenContent = token.Content.ToLowerInvariant();
                    if (tokenContent == "function" || tokenContent == "filter" || tokenContent == "configuration" || tokenContent == "workflow")
                    {
                        nextTokenIsFunctionName = true;
                    }
                }
            }
            return rootItem;
        }

    }
}
