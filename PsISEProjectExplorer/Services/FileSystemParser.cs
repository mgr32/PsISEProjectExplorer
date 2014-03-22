using PsISEProjectExplorer.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.Services
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

            try
            {
                using (
                    FileStream fs = File.Open(this.Path, FileMode.Open, FileAccess.Read,
                        FileShare.Delete | FileShare.ReadWrite))
                {
                    using (BufferedStream bs = new BufferedStream(fs))
                    {
                        using (StreamReader sr = new StreamReader(bs))
                        {
                            this.FileContents = sr.ReadToEnd();
                        }
                    }
                }
                this.PowershellFunctions = PowershellTokenizer.GetFunctions(this.FileContents);
            }
            catch (IOException e)
            {
                //TODO: for now just swallowing but need to log!
            }
        }
    }
}
