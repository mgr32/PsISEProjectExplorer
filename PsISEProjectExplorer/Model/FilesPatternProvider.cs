using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.Model
{
    public static class FilesPatternProvider
    {
        public static string POWERSHELL_FILES_PATTERN = "*.ps*1";

        public static Regex POWERSHELL_FILES_REGEX = new Regex(@".*\.ps.*1$", RegexOptions.Compiled);
    }
}
