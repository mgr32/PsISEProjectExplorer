using PsISEProjectExplorer.Model;
using System;
using System.Linq;

namespace PsISEProjectExplorer.Services
{
    [Component]
    public class PowershellFileParser
    {
        private IPowershellTokenizer powershellTokenizer;

        public PowershellFileParser(PowershellTokenizerProvider powershellTokenizerProvider)
        {
            this.powershellTokenizer = powershellTokenizerProvider.GetPowershellTokenizer();
        }

        public PowershellParseResult ParseFile(string path, bool isDirectory, bool isExcluded, string errorMessage)
        {
            if (isExcluded || isDirectory || !FilesPatternProvider.IsPowershellFile(path))
            {
                return new PowershellParseResult(null, errorMessage, path, null, isDirectory, isExcluded);
            }
            string fileContents;
            try                
            {
                fileContents = FileReader.ReadFileAsString(path);
            }
            catch (Exception e)
            {
                return new PowershellParseResult(null, e.Message, path, null, isDirectory, isExcluded);
            }
            if (fileContents != null)
            {
               var rootPowershellItem = powershellTokenizer.GetPowershellItems(path, fileContents);
               return new PowershellParseResult(rootPowershellItem, errorMessage, path, fileContents, isDirectory, isExcluded);
            }
            return new PowershellParseResult(null, errorMessage, path, fileContents, isDirectory, isExcluded);
        }

    }
}
