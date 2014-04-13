using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PsISEProjectExplorer.Services
{
    public class FilesPatternProvider
    {
        private const string PowershellFilesPattern = "*.ps*1";

        private const string AllFilesPattern = "*";

        private static readonly Regex PowershellFilesRegex = new Regex(@".*\.ps.*1$", RegexOptions.Compiled);

        public bool IncludeAllFiles { get; set; }

        private ISet<string> AdditionalPaths { get; set; }

        public FilesPatternProvider(bool includeAllFiles)
        {
            this.IncludeAllFiles = includeAllFiles;
            this.AdditionalPaths = new HashSet<string>();
        }

        public bool DoesFileMatch(string fileName)
        {
            if (this.IncludeAllFiles)
            {
                return true;
            }
            return PowershellFilesRegex.IsMatch(fileName);
        }

        public string GetFilesPattern()
        {
            return this.IncludeAllFiles ? AllFilesPattern : PowershellFilesPattern;
        }

        public void AddAdditionalPath(string path)
        {
            this.AdditionalPaths.Add(path);
        }

        public bool IsInAdditonalPaths(string path)
        {
            return this.AdditionalPaths.Contains(path);
        }

        public void ClearAdditionalPaths()
        {
            this.AdditionalPaths.Clear();
        }
    }
}
