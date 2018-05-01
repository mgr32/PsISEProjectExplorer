using PsISEProjectExplorer.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PsISEProjectExplorer.Services
{
    [Component]
    public class FilesPatternProvider
    {
        private const string PowershellFilesPattern = "*.ps*1";

        private const string AllFilesPattern = "*";

        private static readonly Regex PowershellFilesRegex = new Regex(@".*\.ps.*1$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex OtherIncludedFilesRegex = new Regex(@".*\.ps.*1.+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex PowershellModulesRegex = new Regex(@".*\.ps(d|m)1$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex ExcludeRegex = new Regex(@"\\(\.git|\.svn|\.vs)($|\\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public bool IncludeAllFiles { get; set; }

        public IndexingMode IndexFilesMode { get; set; }

        public IEnumerable<string> ExcludePaths { get; set; }

        private ISet<string> AdditionalPaths { get; set; }

        public FilesPatternProvider()
        {
            this.AdditionalPaths = new HashSet<string>();
        }

        public static bool IsPowershellFile(string fileName)
        {
            return PowershellFilesRegex.IsMatch(fileName);
        }

        public bool DoesFileMatch(string fileName)
        {
            return (this.IncludeAllFiles || PowershellFilesRegex.IsMatch(fileName) || OtherIncludedFilesRegex.IsMatch(fileName)) && !ExcludeRegex.IsMatch(fileName) && !IsHiddenSystem(fileName);
        }

        public bool IsModuleFile(string fileName)
        {
            return (PowershellModulesRegex.IsMatch(fileName));
        }

        public bool DoesDirectoryMatch(string dirName)
        {
            return !ExcludeRegex.IsMatch(dirName) && !IsHiddenSystem(dirName);
        }

        public bool IsExcludedByUser(string path)
        {
            return ExcludePaths.Any(e => path.StartsWith(e));
        }

        public bool IsExcludedFromIndexing(string path)
        {
            if (IsExcludedByUser(path))
            {
                return true;
            }
            if (IndexFilesMode == IndexingMode.ALL_FILES || IsDirectory(path))
            {
                return false;
            }
            if (IndexFilesMode == IndexingMode.NO_FILES)
            {
                return true;
            }
            return !IsLocal(path);
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

        private bool IsLocal(string path)
        {
            var attributes = GetAttributes(path);
            bool isUnpinned = ((int)attributes & 0x100000) == 0x100000; // currently undocumented - see https://superuser.com/questions/44812/windows-explorers-file-attribute-column-values/44820
            bool isOffline = (attributes & FileAttributes.Offline) == FileAttributes.Offline;
            return !isUnpinned && !isOffline;
        }

        private bool IsHiddenSystem(string path)
        {
            var attributes = GetAttributes(path);
            bool isHidden = (attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
            bool isSystem = (attributes & FileAttributes.System) == FileAttributes.System;
            return isHidden && isSystem;
        }

        private bool IsDirectory(string path)
        {
            return (GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory;
        }

        private FileAttributes GetAttributes(string path)
        {
            try
            {
                return File.GetAttributes(path);
            }
            catch
            {
                return FileAttributes.Normal;
            }
        }
    }
}
