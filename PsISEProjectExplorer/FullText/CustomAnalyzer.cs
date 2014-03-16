using Lucene.Net.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.FullText
{
    /// <summary>
    /// Custom Lucene Analyzer - like SimpleAnalyzer but treats numbers as text.
    /// </summary>
    public class CustomAnalyzer : Analyzer
    {

        public override TokenStream TokenStream(System.String fieldName, System.IO.TextReader reader)
        {
            return new CustomTokenizer(reader);
        }

        public override TokenStream ReusableTokenStream(System.String fieldName, System.IO.TextReader reader)
        {
            var tokenizer = (Tokenizer)PreviousTokenStream;
            if (tokenizer == null)
            {
                tokenizer = new CustomTokenizer(reader);
                PreviousTokenStream = tokenizer;
            }
            else
                tokenizer.Reset(reader);
            return tokenizer;
        }

    }
}
