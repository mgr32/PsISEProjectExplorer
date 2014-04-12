using System.Text.RegularExpressions;

namespace PsISEProjectExplorer.Model
{
    public static class FilesPatternProvider
    {
        private const string PowershellFilesPattern = "*.ps*1";

        private const string AllFilesPattern = "*";

        private static readonly Regex PowershellFilesRegex = new Regex(@".*\.ps.*1$", RegexOptions.Compiled);

        public static bool DoesFileMatch(string fileName, bool includeAllFiles)
        {
            if (includeAllFiles)
            {
                return true;
            }
            return PowershellFilesRegex.IsMatch(fileName);
        }

        public static string GetFilesPattern(bool includeAllFiles)
        {
            return includeAllFiles ? AllFilesPattern : PowershellFilesPattern;
        }
    }
}
