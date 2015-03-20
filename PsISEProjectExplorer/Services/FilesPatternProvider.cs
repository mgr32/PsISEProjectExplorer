using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace PsISEProjectExplorer.Services
{
    public class FilesPatternProvider
    {
        private const string PowershellFilesPattern = "*.ps*1";

        private const string AllFilesPattern = "*";

        private static readonly Regex PowershellFilesRegex = new Regex(@".*\.ps.*1$", RegexOptions.Compiled);

        private static readonly Regex ExcludeRegex = new Regex(@"\\(\.git|\.svn)($|\\)");

        public bool IncludeAllFiles { get; set; }

        private ISet<string> AdditionalPaths { get; set; }

        public FilesPatternProvider(bool includeAllFiles)
        {
			IncludeAllFiles = includeAllFiles;
			AdditionalPaths = new HashSet<string>();
        }

        public static bool IsPowershellFile(string fileName)
        {
            return PowershellFilesRegex.IsMatch(fileName);
        }

        public bool DoesFileMatch(string fileName)
        {
            return (IncludeAllFiles || PowershellFilesRegex.IsMatch(fileName)) && !ExcludeRegex.IsMatch(fileName) && !IsReparsePointOrHiddenSystem(fileName);
        }

        public bool DoesDirectoryMatch(string dirName)
        {
            return !ExcludeRegex.IsMatch(dirName) && !IsReparsePointOrHiddenSystem(dirName);
        }

        public string GetFilesPattern()
        {
            return IncludeAllFiles ? AllFilesPattern : PowershellFilesPattern;
        }

        public void AddAdditionalPath(string path)
        {
			AdditionalPaths.Add(path);
        }

        public void RemoveAdditionalPath(string path)
        {
			AdditionalPaths.Remove(path);
        }

        public bool IsInAdditonalPaths(string path)
        {
            return AdditionalPaths.Contains(path);
        }

        public void ClearAdditionalPaths()
        {
			AdditionalPaths.Clear();
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
