using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.Model
{
    public class PowershellParseResult
    {
        public PowershellItem RootPowershellItem { get; private set; }

        public string ErrorMessage { get; set; }

        public string Path { get; private set; }

        public string FileContents { get; private set; }

        public bool IsDirectory { get; private set; }

        public bool IsExcluded { get; private set; }

        public PowershellParseResult(PowershellItem rootPowershellItem, string errorMessage, string path, string fileContents, bool isDirectory, bool isExcluded)
        {
            this.RootPowershellItem = rootPowershellItem;
            if (rootPowershellItem != null)
            {
                this.ErrorMessage = this.RootPowershellItem.ParsingErrors;
            }
            else
            {
                this.ErrorMessage = errorMessage;
            }
            this.Path = path;
            this.FileContents = fileContents;
            this.IsDirectory = isDirectory;
            this.IsExcluded = isExcluded;
        }
        

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

    }
}
