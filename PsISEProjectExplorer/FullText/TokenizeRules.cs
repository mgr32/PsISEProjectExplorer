using System.Linq;

namespace PsISEProjectExplorer.FullText
{
    public class TokenizeRules
    {
        public bool IsTokenChar(char c)
        {
            return !System.Char.IsWhiteSpace(c) && !this.IsPhraseChar(c);
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
