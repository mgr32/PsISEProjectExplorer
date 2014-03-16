using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace ProjectExplorer.DocHierarchy.HierarchyLogic
{
    public class FileSystemParser
    {
        public string Path { get; private set; }

        public string FileContents { get; private set; }

        public IEnumerable<PowershellFunction> PowershellFunctions { get; private set; }

        public bool IsDirectory { get; private set; }

        public string FileName
        {
            get
            {
                return Path.Split('\\').Last();
            }
        }
        
        public FileSystemParser(string path, bool isDirectory)
        {
            this.Path = path;
            this.IsDirectory = isDirectory;
            this.PowershellFunctions = new List<PowershellFunction>();
            if (!this.IsDirectory)
            {
                this.ParseFile();
            }
            else
            {
                this.FileContents = string.Empty;
            }
        }

        private void ParseFile()
        {
            // TODO: error handling
            using (FileStream fs = File.Open(this.Path, FileMode.Open))
            {
                using (BufferedStream bs = new BufferedStream(fs))
                {
                    using (StreamReader sr = new StreamReader(bs))
                    {
                        this.FileContents = sr.ReadToEnd();
                    }
                }
            }
            Collection<PSParseError> errors;
            IEnumerable<PSToken> tokens = PSParser.Tokenize(this.FileContents, out errors);
            IList<PowershellFunction> functions = (IList<PowershellFunction>)this.PowershellFunctions;
            bool nextTokenIsFunction = false;
            foreach (PSToken token in tokens)
            {
                if (nextTokenIsFunction)
                {
                    functions.Add(new PowershellFunction(token.Content, token.StartLine, token.StartColumn));
                    nextTokenIsFunction = false;
                }
                if (token.Type == PSTokenType.Keyword)
                {
                    string tokenContent = token.Content.ToLowerInvariant();
                    if (tokenContent == "function" || tokenContent == "filter")
                    {
                        nextTokenIsFunction = true;
                    }
                }
            }
        }
    }
}
