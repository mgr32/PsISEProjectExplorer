using Lucene.Net.Analysis;
using Lucene.Net.Util;

namespace PsISEProjectExplorer.FullText
{
    public class CustomTokenizer : CharTokenizer
    {
        
        /// <summary>Construct a new CustomTokenizer. </summary>
		public CustomTokenizer(System.IO.TextReader @in):base(@in)
		{
		}

        /// <summary>Construct a new CustomTokenizer using a given <see cref="AttributeSource" />. </summary>
		public CustomTokenizer(AttributeSource source, System.IO.TextReader @in)
			: base(source, @in)
		{
		}

        /// <summary>Construct a new CustomTokenizer using a given <see cref="Lucene.Net.Util.AttributeSource.AttributeFactory" />. </summary>
        public CustomTokenizer(AttributeFactory factory, System.IO.TextReader @in)
			: base(factory, @in)
		{
		}

        /// <summary>Collects only characters which satisfy
        /// <see cref="char.IsLetter(char)" />.
        /// </summary>
        protected override bool IsTokenChar(char c)
        {
            return TokenizeRules.IsTokenChar(c);
        }

        /// <summary>Converts char to lower case
        /// <see cref="char.ToLower(char)" />.
        /// </summary>
        protected override char Normalize(char c)
        {
            return TokenizeRules.Normalize(c);
        }
    }
}
