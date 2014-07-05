using PsISEProjectExplorer.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PsISEProjectExplorer.Services
{
    public class PowershellFileParser
    {
        public string Path { get; private set; }

        public string FileContents { get; private set; }

        public IEnumerable<PowershellItem> PowershellItems { get; private set; }

        public bool IsDirectory { get; private set; }

        public string ErrorMessage { get; set; }

        public string FileName
        {
            get
            {
                return Path.Split('\\').Last();
            }
        }

        public bool IsValid
        {
            get
            {
                return ErrorMessage != null;
            }
        }

        public PowershellFileParser(string path, bool isDirectory)
            : this(path, isDirectory, null)
        {

        }
        
        public PowershellFileParser(string path, bool isDirectory, string errorMessage)
        {
            this.Path = path;
            this.IsDirectory = isDirectory;
            this.ErrorMessage = errorMessage;
            this.PowershellItems = new List<PowershellItem>();
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
            try
            {
                this.FileContents = FileReader.ReadFileAsString(this.Path);
            }
            catch (Exception e)
            {
                this.ErrorMessage = e.Message;
                return;
            }
            if (this.FileContents != null)
            {
               this.PowershellItems = PowershellTokenizer.GetFunctions(this.FileContents);
            }
        }
    }
}
