using PsISEProjectExplorer.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;

namespace PsISEProjectExplorer.Services
{
    public class PowershellTokenizer
    {
        public static IList<PowershellFunction> GetFunctions(string contents)
        {
            Collection<PSParseError> errors;
            IEnumerable<PSToken> tokens = PSParser.Tokenize(contents, out errors);
            IList<PowershellFunction> functions = new List<PowershellFunction>();
            bool nextTokenIsFunction = false;
            foreach (PSToken token in tokens)
            {
                if (nextTokenIsFunction)
                {
                    functions.Add(new PowershellFunction(token.Content, token.StartLine, token.StartColumn));
                    nextTokenIsFunction = false;
                }
                if (token.Type == PSTokenType.Keyword)
                {
                    string tokenContent = token.Content.ToLowerInvariant();
                    if (tokenContent == "function" || tokenContent == "filter")
                    {
                        nextTokenIsFunction = true;
                    }
                }
            }
            return functions;
        }

        public static string GetTokenAtColumn(string line, int column)
        {
            Collection<PSParseError> errors;
            IEnumerable<PSToken> tokens = PSParser.Tokenize(line, out errors);
            return tokens.Where(token => token.StartColumn <= column && token.EndColumn >= column).Select(token => token.Content).FirstOrDefault();
        }

    }
}
