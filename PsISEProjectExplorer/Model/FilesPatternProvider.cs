using System.Text.RegularExpressions;

namespace PsISEProjectExplorer.Model
{
    public static class FilesPatternProvider
    {
        public const string PowershellFilesPattern = "*.ps*1";

        public static readonly Regex PowershellFilesRegex = new Regex(@".*\.ps.*1$", RegexOptions.Compiled);
    }
}
