using PsISEProjectExplorer.Model;
using System.Collections.Generic;
using System.Linq;

namespace PsISEProjectExplorer.Services
{
    public class PowershellFileParser
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
        
        public PowershellFileParser(string path, bool isDirectory)
        {
            this.Path = path;
            this.IsDirectory = isDirectory;
            this.PowershellFunctions = new List<PowershellFunction>();
            if (!this.IsDirectory && FilesPatternProvider.IsPowershellFile(path))
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

           this.FileContents = FileReader.ReadFileAsString(this.Path);
           if (this.FileContents != null)
           {
               this.PowershellFunctions = PowershellTokenizer.GetFunctions(this.FileContents);
           }
        }
    }
}
