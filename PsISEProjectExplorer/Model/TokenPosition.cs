
namespace PsISEProjectExplorer.Model
{
    public class TokenPosition
    {
        public int MatchLength { get; private set; }

        public int Line { get; private set; }

        public int Column { get; private set; }

        public TokenPosition(int matchLength, int line, int column)
        {
            this.MatchLength = matchLength;
            this.Line = line;
            this.Column = column;
        }
    }
}
