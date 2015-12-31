using PsISEProjectExplorer.Services;

namespace PsISEProjectExplorer.Model
{
    public class EditorInfo
    {
        public string CurrentLine { get; private set; }

        public int CurrentLineNum { get; private set; }

        public int CurrentColumn { get; private set; }

        public EditorInfo(string currentLine, int currentLineNum, int currentColumn)
        {
            this.CurrentLine = currentLine;
            this.CurrentLineNum = currentLineNum;
            this.CurrentColumn = currentColumn;
        }

        public string GetTokenFromCurrentPosition()
        {
            return PowershellTokenizerProvider.GetPowershellTokenizer().GetTokenAtColumn(this.CurrentLine, this.CurrentColumn);
        }
    }
}
