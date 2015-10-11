using Lucene.Net.Index;
using Lucene.Net.Search;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PsISEProjectExplorer.FullText
{
    /// <summary>
    /// Custom very simple query parser. Can't use the built-in one for example due to treatment of '-' and OR between words.
    /// Maybe in future could be replaced with ANTLR.
    /// </summary>
    public class CustomQueryParser
    { 
        private TokenizeRules TokenizeRules = new TokenizeRules();
    
        public Query Parse(string text, string field)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }
            var tokens = this.Tokenize(text).ToList();
            if (!tokens.Any())
            {
                return null;
            }
            BooleanQuery outerQuery = new BooleanQuery();
            PhraseQuery phraseQuery = null;
            foreach (string token in tokens)
            {
                if ("\"".Equals(token))
                {
                    if (phraseQuery == null)
                    {
                        phraseQuery = new PhraseQuery();
                    }
                    else
                    {
                        outerQuery.Add(phraseQuery, Occur.MUST);
                        phraseQuery = null;
                    }
                }
                else
                {
                    Term term = new Term(field, token);
                    if (phraseQuery != null)
                    {
                        phraseQuery.Add(term);
                    }
                    else
                    {
                        var query = new PrefixQuery(term);
                        outerQuery.Add(query, Occur.MUST);
                    }
                }
            }
            if (phraseQuery != null)
            {
                outerQuery.Add(phraseQuery, Occur.MUST);
            }
            return outerQuery;
        }

        private IEnumerable<string> Tokenize(string text)
        {
            IList<string> tokens = new List<string>();
            int tokenLen = 0;

            int len = text.Length;
            var buf = new char[255];
            for (int i = 0; i <= len; i++)
            {
                char c = '\x00';
                if (i != len) 
                {
                    c = this.TokenizeRules.Normalize(text[i]);
                    if (this.TokenizeRules.IsTokenChar(c))
                    {
                        buf[tokenLen] = c;
                        tokenLen++;
                        if (tokenLen > 255)
                        {
                            throw new InvalidOperationException("Token longer than 255 characters.");
                        }
                        continue;
                    }                    
                }
                
                // we're not interested in tokens shorter than 3 chars
                if (tokenLen >= 3)
                {
                    tokens.Add(new string(buf, 0, tokenLen));
                }
                tokenLen = 0;
                if (this.TokenizeRules.IsPhraseChar(c))
                {
                    tokens.Add("\"");
                }                
            }
            return tokens;
        }
    }
}
