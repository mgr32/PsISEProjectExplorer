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
			CurrentLine = currentLine;
			CurrentLineNum = currentLineNum;
			CurrentColumn = currentColumn;
        }

        public string GetTokenFromCurrentPosition()
        {
            return PowershellTokenizer.GetTokenAtColumn(CurrentLine, CurrentColumn);
        }
    }
}
