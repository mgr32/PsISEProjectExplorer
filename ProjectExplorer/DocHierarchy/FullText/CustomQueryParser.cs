using Lucene.Net.Index;
using Lucene.Net.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectExplorer.DocHierarchy.FullText
{
    /// <summary>
    /// Custom very simple query parser. Can't use the built-in one for example due to treatment of '-' and OR between words.
    /// Maybe in future could be replaced with ANTLR.
    /// </summary>
    public class CustomQueryParser
    {
        private static char[] TOKEN_SEPARATORS = new char[] { ' ', '-', '.', ',' };

        public Query Parse(string text, string field)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }
            var tokens = this.Tokenize(text.ToLowerInvariant());
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
            return text.Split(TOKEN_SEPARATORS).Where(token => token.Length > 1).ToList();
        }
    }
}
