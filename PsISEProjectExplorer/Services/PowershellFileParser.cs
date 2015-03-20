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
			Path = path;
			IsDirectory = isDirectory;
			ErrorMessage = errorMessage;
            if (!IsDirectory && FilesPatternProvider.IsPowershellFile(path))
            {
				ParseFile();
            }
            else
            {
				FileContents = string.Empty;
            }
        }

        private void ParseFile()
        {
            try
            {
				FileContents = FileReader.ReadFileAsString(Path);
            }
            catch (Exception e)
            {
				ErrorMessage = e.Message;
                return;
            }
            if (FileContents != null)
            {
				RootPowershellItem = PowershellTokenizer.GetPowershellItems(Path, FileContents);
				ErrorMessage = RootPowershellItem.ParsingErrors;
            }
        }
    }
}
