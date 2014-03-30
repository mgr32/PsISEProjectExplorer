
namespace PsISEProjectExplorer.Model
{
    public class LineInfo
    {
        public string LineText { get; private set; }

        public int LineNumber { get; private set; }

        public LineInfo(string lineText, int lineNumber)
        {
            this.LineText = lineText;
            this.LineNumber = lineNumber;
        }
    }
}
