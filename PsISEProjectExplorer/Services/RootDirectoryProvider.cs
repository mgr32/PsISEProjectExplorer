using PsISEProjectExplorer.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PsISEProjectExplorer.Services
{
    public static class RootDirectoryProvider
    {
        public static string GetRootDirectoryToSearch(string filePath)
        {
            if (String.IsNullOrEmpty(filePath))
            {
                return null;
            }
            string driveRoot = Path.GetPathRoot(filePath).ToLowerInvariant();
            string rootDir = Path.GetDirectoryName(filePath);
            if (String.IsNullOrEmpty(rootDir))
            {
                return null;
            }
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
                    if (FilesPatternProvider.DoesFileMatch(file, false))
                    {
                        rootDir = currentDir;
                    }
                }
            }
        }
    }
}
