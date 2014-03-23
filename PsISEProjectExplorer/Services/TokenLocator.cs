using NLog;
using PsISEProjectExplorer.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.Services
{
    public class TokenLocator
    {

        public static TokenPosition LocateNextToken(string filePath, string searchText, EditorInfo editorInfo)
        {
            return LocateSubtoken(filePath, searchText, true, editorInfo);
        }

        public static TokenPosition LocateSubtoken(string filePath, string searchText)
        {
            return LocateSubtoken(filePath, searchText, false, null);
        }


        private static TokenPosition LocateSubtoken(string filePath, string searchText, bool fullTokensOnly, EditorInfo editorInfo)
        {
            int startLine = 1;
            int startColumnInFirstLine = 0;
            if (editorInfo != null)
            {
                startLine = editorInfo.CurrentLineNum;
                startColumnInFirstLine = editorInfo.CurrentColumn;
            }
            int queryLen = searchText.Length;
            TokenPosition bestSubtokenPosition = new TokenPosition(-1, 0, 0);
            foreach (LineInfo lineInfo in FileReader.ReadFileAsEnumerableWithWrap(filePath, startLine))
            {
                int columnsToIgnore = (lineInfo.LineNumber == startLine ? startColumnInFirstLine : 0);
                TokenPosition tokenPos = GetLongestSubtoken(lineInfo.LineNumber, lineInfo.LineText, searchText, columnsToIgnore);
                if (tokenPos.MatchLength > bestSubtokenPosition.MatchLength)
                {
                    bestSubtokenPosition = tokenPos;
                    if (tokenPos.MatchLength == queryLen)
                    {
                        break;
                    }
                }
            }
            if (fullTokensOnly && bestSubtokenPosition.MatchLength != queryLen)
            {
                return new TokenPosition(-1, 0, 0);
            }
            return bestSubtokenPosition;
        }

        private static TokenPosition GetLongestSubtoken(int lineNum, string line, string searchText, int columnsToIgnore)
        {
            int queryPosMax = 0;
            int column = -1;
            int queryPos = 0;
            int queryLen = searchText.Length;
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
