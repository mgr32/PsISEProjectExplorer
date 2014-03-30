using System.Linq;

namespace PsISEProjectExplorer.FullText
{
    public class TokenizeRules
    {
        private static readonly char[] TokenSeparators = { '.', ',', ':', '(', ')', '[', ']', '{', '}', '\\', '/', '`' };

        public static bool IsTokenChar(char c)
        {
            return !System.Char.IsWhiteSpace(c) && !TokenSeparators.Contains(c);
        }

        public static char Normalize(char c)
        {
            return System.Char.ToLower(c);
        }
    }
}
