using Lucene.Net.Analysis;

namespace PsISEProjectExplorer.FullText
{
    public class CustomTokenizer : CharTokenizer
    {

        private TokenizeRules TokenizeRules = new TokenizeRules();

        /// <summary>Construct a new CustomTokenizer. </summary>
        public CustomTokenizer(System.IO.TextReader @in):base(@in)
		{
		}

        /// <summary>Collects only characters which satisfy
        /// <see cref="char.IsLetter(char)" />.
        /// </summary>
        protected override bool IsTokenChar(char c)
        {
            return this.TokenizeRules.IsTokenChar(c);
        }

        /// <summary>Converts char to lower case
        /// <see cref="char.ToLower(char)" />.
        /// </summary>
        protected override char Normalize(char c)
        {
            return this.TokenizeRules.Normalize(c);
        }
    }
}
