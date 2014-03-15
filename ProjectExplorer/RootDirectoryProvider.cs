using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProjectExplorer
{
    public class RootDirectoryProvider
    {
        private static Regex ROOT_DIR_SEARCH_END_PATTERN = new Regex(@"(\.psm1|\.psd1)$", RegexOptions.Compiled);
        private static Regex ROOT_DIR_SEARCH_INTERMEDIATE_PATTERN = new Regex(@"\.ps1$", RegexOptions.Compiled);

        public static string GetRootDirectoryToSearch(string currentFile)
        {

            string rootDir = Path.GetDirectoryName(currentFile);
            string currentDir = rootDir;
            while (true)
            {
                currentDir = Directory.GetParent(currentDir).FullName;
                if (currentDir == null)
                {
                    return rootDir;
                }
                IList<string> allFilesInCurrentDir = Directory.GetFiles(currentDir).ToList();
                bool onlyDirectories = true;
                foreach (string file in allFilesInCurrentDir)
                {
                    if (ROOT_DIR_SEARCH_END_PATTERN.IsMatch(file))
                    {
                        // end pattern -> return
                        return currentDir;
                    }
                    if (ROOT_DIR_SEARCH_INTERMEDIATE_PATTERN.IsMatch(file))
                    {
                        // intermediate pattern -> go further
                        rootDir = currentDir;
                        continue;
                    }
                    if ((File.GetAttributes(file) & FileAttributes.Directory) != FileAttributes.Directory)
                    {
                        onlyDirectories = false;
                    }
                }
                if (!onlyDirectories)
                {
                    // no powershell files and there are some other files -> return last rootDir
                    return rootDir;
                }
                else
                {
                    // some powershell files -> go further
                    rootDir = currentDir;
                    continue;
                }
            }
        }
    }
}
