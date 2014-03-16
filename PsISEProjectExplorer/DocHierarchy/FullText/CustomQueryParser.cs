using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Tokenattributes;
using Lucene.Net.Index;
using Lucene.Net.Search;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.DocHierarchy.FullText
{
    /// <summary>
    /// Custom very simple query parser. Can't use the built-in one for example due to treatment of '-' and OR between words.
    /// Maybe in future could be replaced with ANTLR.
    /// </summary>
    public class CustomQueryParser
    {
        public Analyzer Analyzer { get; private set; }

        public CustomQueryParser(Analyzer analyzer)
        {
            this.Analyzer = analyzer;
        }

        public Query Parse(string text, string field)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }
            var tokens = this.Tokenize(text);
            if (!tokens.Any())
            {
                return null;
            }
            BooleanQuery outerQuery = new BooleanQuery();
            foreach (string token in tokens)
            {
                PrefixQuery query = new PrefixQuery(new Term(field, token));
                outerQuery.Add(query, Occur.MUST);
            }
            return outerQuery;
        }

        private IEnumerable<string> Tokenize(string text)
        {
            IList<string> tokens = new List<string>();
            int startPos = 0;
            int tokenLen = 0;

            int len = text.Length;
            for (int i = 0; i <= len; i++)
            {
                char c = '\x00';
                if (i != len) 
                {
                    c = TokenizeRules.Normalize(text[i]);
                }
                if (i != len && TokenizeRules.IsTokenChar(c))
                {
                    tokenLen++;
                }
                else
                {
                    // we're not interested in tokens shorter than 2 chars
                    if (tokenLen >= 1)
                    {
                        // not optimal but this is only for short input string
                        tokens.Add(text.Substring(startPos, tokenLen));
                    }
                    startPos = startPos + tokenLen + 1;
                    tokenLen = 0;
                }
            }
            return tokens;
        }
    }
}
