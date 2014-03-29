using PsISEProjectExplorer.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.Services
{
    public class RootDirectoryProvider
    {
        public static string GetRootDirectoryToSearch(string filePath)
        {
            if (String.IsNullOrEmpty(filePath))
            {
                return null;
            }
            string driveRoot = Path.GetPathRoot(filePath).ToLowerInvariant();
            string rootDir = Path.GetDirectoryName(filePath);
            string currentDir = rootDir;
            while (true)
            {
                var currentDirInfo = Directory.GetParent(currentDir);
                if (currentDirInfo == null || currentDirInfo.FullName.ToLowerInvariant() == driveRoot)
                {
                    return (rootDir.ToLowerInvariant() == driveRoot ? null : rootDir);
                }
                currentDir = currentDirInfo.FullName;
                IList<string> allFilesInCurrentDir;
                try
                {
                    allFilesInCurrentDir = Directory.GetFiles(currentDir).ToList();
                }
                catch (IOException)
                {
                    return null;
                }
                foreach (string file in allFilesInCurrentDir)
                {
                    if (FilesPatternProvider.POWERSHELL_FILES_REGEX.IsMatch(file))
                    {
                        rootDir = currentDir;
                        continue;
                    }
                }
            }
        }
    }
}
