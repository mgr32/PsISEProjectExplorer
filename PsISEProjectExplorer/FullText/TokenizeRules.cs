using System.Linq;

namespace PsISEProjectExplorer.FullText
{
    public class TokenizeRules
    {
        private static readonly char[] TokenSeparators = { '.', ',', ':', '(', ')', '[', ']', '{', '}', '\\', '/', '`', '\'', '"' };

        // note this is used both in CustomQueryParser (for parsing user input) and CustomTokenizer (for parsing document)
        public bool IsTokenChar(char c)
        {
            return !System.Char.IsWhiteSpace(c) && !TokenSeparators.Contains(c);
        }

        public bool IsPhraseChar(char c)
        {
            return '"'.Equals(c);
        }

        public char Normalize(char c)
        {
            return System.Char.ToLower(c);
        }
    }
}
