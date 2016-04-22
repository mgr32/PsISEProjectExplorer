using PsISEProjectExplorer.Model;
using System.Text.RegularExpressions;

namespace PsISEProjectExplorer.Services
{
    [Component]
    public class TokenLocator
    {

        private FileReader FileReader { get; set; }

        public TokenLocator(FileReader fileReader)
        {
            this.FileReader = fileReader;
        }

        public TokenPosition LocateNextToken(string filePath, SearchOptions searchOptions, EditorInfo editorInfo)
        {
            return LocateSubtoken(filePath, searchOptions, true, editorInfo);
        }

        public TokenPosition LocateSubtoken(string filePath, SearchOptions searchOptions)
        {
            return LocateSubtoken(filePath, searchOptions, false, null);
        }


        private TokenPosition LocateSubtoken(string filePath, SearchOptions searchOptions, bool fullTokensOnly, EditorInfo editorInfo)
        {
            int startLine = 1;
            int startColumnInFirstLine = 0;
            if (editorInfo != null)
            {
                startLine = editorInfo.CurrentLineNum;
                startColumnInFirstLine = editorInfo.CurrentColumn;
            }
            string searchText = searchOptions.SearchText;
            if (!searchOptions.SearchRegex)
            {
                searchText = searchText.Replace("\"", string.Empty);
            }
            int queryLen = searchText.Length;
            var bestSubtokenPosition = new TokenPosition(-1, 0, 0);
            Regex regex = searchOptions.SearchRegex ? new Regex(searchText, RegexOptions.Compiled | RegexOptions.IgnoreCase) : null;
            bool firstLine = true;
            foreach (LineInfo lineInfo in FileReader.ReadFileAsEnumerableWithWrap(filePath, startLine))
            {
                int columnsToIgnore = (firstLine ? startColumnInFirstLine : 0);
                firstLine = false;
                TokenPosition tokenPos = GetLongestSubtoken(lineInfo.LineNumber, lineInfo.LineText, searchText, regex, columnsToIgnore);
                if (tokenPos.MatchLength > bestSubtokenPosition.MatchLength)
                {
                    bestSubtokenPosition = tokenPos;
                    if (tokenPos.MatchLength == queryLen)
                    {
                        break;
                    }
                }
            }
            if (!searchOptions.SearchRegex && fullTokensOnly && bestSubtokenPosition.MatchLength != queryLen)
            {
                return new TokenPosition(-1, 0, 0);
            }
            return bestSubtokenPosition;
        }

        private TokenPosition GetLongestSubtoken(int lineNum, string line, string searchText, Regex searchRegex, int columnsToIgnore)
        {
            int queryPosMax = 0;
            int column = -1;
            int queryPos = 0;
            int queryLen = searchText.Length;

            if (searchRegex != null)
            {
                if (line.Length > columnsToIgnore)
                {
                    Match match = searchRegex.Match(line.Substring(columnsToIgnore));
                    if (match.Success)
                    {
                        return new TokenPosition(match.Length, lineNum, match.Index + columnsToIgnore + 1);
                    }
                }
                return new TokenPosition(queryPosMax, lineNum, column);
            }
            for (int i = columnsToIgnore; i < line.Length; i++)
            {
                if (char.ToLowerInvariant(line[i]) == char.ToLowerInvariant(searchText[queryPos]))
                { 
                    queryPos++;
                    if (queryPos > queryPosMax) 
                    {
                        queryPosMax = queryPos;
                        column = i - queryPos + 2;
                    }
                    if (queryPos == queryLen)
                    {
                        break;
                    }
                } 
                else 
                {
                    queryPos = 0;
                }
            }

            return new TokenPosition(queryPosMax, lineNum, column);
        }
    }
}
