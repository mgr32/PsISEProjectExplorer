using PsISEProjectExplorer.Model;
using System;
using System.Linq;

namespace PsISEProjectExplorer.Services
{
    public class PowershellFileParser
    {
        public string Path { get; private set; }

        public string FileContents { get; private set; }

        public PowershellItem RootPowershellItem { get; private set; }

        public bool IsDirectory { get; private set; }

        public bool IsExcluded { get; private set; }

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
            : this(path, isDirectory, false, null)
        {

        }

        public PowershellFileParser(string path, bool isDirectory, bool isExcluded)
            : this(path, isDirectory, isExcluded, null)
        {

        }

        public PowershellFileParser(string path, bool isDirectory, string errorMessage)
            : this(path, isDirectory, false, errorMessage)
        {

        }

        public PowershellFileParser(string path, bool isDirectory, bool isExcluded, string errorMessage)
        {
            this.Path = path;
            this.IsDirectory = isDirectory;
            this.IsExcluded = isExcluded;
            this.ErrorMessage = errorMessage;
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
               this.RootPowershellItem = PowershellTokenizer.GetPowershellItems(this.Path, this.FileContents);
               this.ErrorMessage = this.RootPowershellItem.ParsingErrors;
            }
        }
    }
}
