using PsISEProjectExplorer.Model;
using System;

namespace PsISEProjectExplorer.Services
{
    [Component]
    public class PowershellFileParser
    {
        private readonly IPowershellTokenizer powershellTokenizer;

        private readonly FileReader fileReader;

        private readonly PowershellTreeRestructurer powershellTreeRestructurer;

        public PowershellFileParser(PowershellTokenizerProvider powershellTokenizerProvider, FileReader fileReader, PowershellTreeRestructurer powershellTreeRestructurer)
        {
            this.powershellTokenizer = powershellTokenizerProvider.PowershellTokenizer;
            this.fileReader = fileReader;
            this.powershellTreeRestructurer = powershellTreeRestructurer;
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
                fileContents = fileReader.ReadFileAsString(path);
            }
            catch (Exception e)
            {
                return new PowershellParseResult(null, e.Message, path, null, isDirectory, isExcluded);
            }
            if (fileContents != null)
            {
               var rootPowershellItem = this.powershellTokenizer.GetPowershellItems(path, fileContents);
               rootPowershellItem = this.powershellTreeRestructurer.AddRegions(rootPowershellItem, fileContents);
               return new PowershellParseResult(rootPowershellItem, errorMessage, path, fileContents, isDirectory, isExcluded);
            }
            return new PowershellParseResult(null, errorMessage, path, fileContents, isDirectory, isExcluded);
        }

    }
}
