using PsISEProjectExplorer.Services;
using System.Linq;

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

        public string GetFunctionInCurrentLine()
        {
            return PowershellTokenizer.GetFunctions(this.CurrentLine).Select(f => f.Name).FirstOrDefault();
        }

        public string GetTokenFromCurrentPosition()
        {
            return PowershellTokenizer.GetTokenAtColumn(this.CurrentLine, this.CurrentColumn);
        }
    }
}
