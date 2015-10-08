using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace PsISEProjectExplorer.Services
{
    public class FilesPatternProvider
    {
        private const string PowershellFilesPattern = "*.ps*1";

        private const string AllFilesPattern = "*";

        private static readonly Regex PowershellFilesRegex = new Regex(@".*\.ps.*1.*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex PowershellModulesRegex = new Regex(@".*\.ps(d|m)1$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex ExcludeRegex = new Regex(@"\\(\.git|\.svn|\.vs)($|\\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public bool IncludeAllFiles { get; set; }

        private ISet<string> AdditionalPaths { get; set; }

        public FilesPatternProvider(bool includeAllFiles)
        {
            this.IncludeAllFiles = includeAllFiles;
            this.AdditionalPaths = new HashSet<string>();
        }

        public static bool IsPowershellFile(string fileName)
        {
            return PowershellFilesRegex.IsMatch(fileName);
        }

        public bool DoesFileMatch(string fileName)
        {
            return (this.IncludeAllFiles || PowershellFilesRegex.IsMatch(fileName)) && !ExcludeRegex.IsMatch(fileName) && !IsReparsePointOrHiddenSystem(fileName);
        }

        public bool IsModuleFile(string fileName)
        {
            return (PowershellModulesRegex.IsMatch(fileName));
        }

        public bool DoesDirectoryMatch(string dirName)
        {
            return !ExcludeRegex.IsMatch(dirName) && !IsReparsePointOrHiddenSystem(dirName);
        }

        public string GetFilesPattern()
        {
            return this.IncludeAllFiles ? AllFilesPattern : PowershellFilesPattern;
        }

        public void AddAdditionalPath(string path)
        {
            this.AdditionalPaths.Add(path);
        }

        public void RemoveAdditionalPath(string path)
        {
            this.AdditionalPaths.Remove(path);
        }

        public bool IsInAdditonalPaths(string path)
        {
            return this.AdditionalPaths.Contains(path);
        }

        public void ClearAdditionalPaths()
        {
            this.AdditionalPaths.Clear();
        }

        private bool IsReparsePointOrHiddenSystem(string path)
        {
            try
            {
                var attributes = File.GetAttributes(path);
                return (attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint ||
                    ((attributes & FileAttributes.Hidden) == FileAttributes.Hidden &&
                     (attributes & FileAttributes.System) == FileAttributes.System);
            }
            catch
            {
                return false;
            }
        }
    }
}
