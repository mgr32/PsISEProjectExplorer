using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.FullText
{
    public class TokenizeRules
    {
        private static char[] TOKEN_SEPARATORS = new char[] { '.', ',', ':', '(', ')', '[', ']', '{', '}', '\\', '/', '`' };

        public static bool IsTokenChar(char c)
        {
            return !System.Char.IsWhiteSpace(c) && !TOKEN_SEPARATORS.Contains(c);
        }

        public static char Normalize(char c)
        {
            return System.Char.ToLower(c);
        }
    }
}
