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
            var filesPatternProvider = new FilesPatternProvider(false, Enumerable.Empty<string>());
            string currentDir = rootDir;
            while (true)
            {
                var currentDirInfo = Directory.GetParent(currentDir);
                if (currentDirInfo == null || currentDirInfo.FullName.ToLowerInvariant() == driveRoot)
                {
                    // TODO: why root dir is disallowed?
                    return (rootDir.ToLowerInvariant() == driveRoot ? null : rootDir);
                }
                currentDir = currentDirInfo.FullName;
                if (!filesPatternProvider.DoesDirectoryMatch(currentDir))
                {
                    continue;
                }
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
                    if (filesPatternProvider.DoesFileMatch(file))
                    {
                        rootDir = currentDir;
                    }
                    if (filesPatternProvider.IsModuleFile(file))
                    {
                        // TODO: why root dir is disallowed?
                        return (rootDir.ToLowerInvariant() == driveRoot ? null : rootDir);
                    }
                }

            }
        }
    }
}
