using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PsISEProjectExplorer.Model;

namespace PsISEProjectExplorer.Services
{
    public abstract class PowershellBaseTokenizer : IPowershellTokenizer
    {
        private static readonly Regex importDscRegex = new Regex("Import-DSCResource", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        protected bool removeImportDscResource(string contents, out string newContents)
        {
            // this is fix for performance issue in PSParser.Tokenize - when file contains Import-DSCResource pointing to a non-installed resource, 
            // parsing takes long time and 'Unable to load resource' errors appear
            if (importDscRegex.IsMatch(contents))
            {
                newContents = importDscRegex.Replace(contents, "#Import-DSCResource");
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

        public abstract PowershellItem GetPowershellItems(string path, string contents);
    }
}
