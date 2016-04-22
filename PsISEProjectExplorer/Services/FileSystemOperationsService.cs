using System;
using System.IO;

namespace PsISEProjectExplorer.Services
{
    [Component]
    public class FileSystemOperationsService
    {
        public void RenameFileOrDirectory(string filePath, string newFilePath)
        {
            if (newFilePath == filePath)
            {
                return;
            }
            if (Directory.Exists(filePath)) 
            {
                if (IsSubdirectory(filePath, newFilePath))
                {
                    throw new InvalidOperationException("Cannot move folder - the destination folder cannot be a subfolder of the source folder.");
                }
                Directory.Move(filePath, newFilePath);
            } 
            else if (File.Exists(filePath)) 
            {
                File.Move(filePath, newFilePath);
            }
            else
            {
                throw new InvalidOperationException(String.Format("Path '{0}' does not exist.", filePath));
            }
        }

        public void DeleteFileOrDirectory(string filePath)
        {
            if (Directory.Exists(filePath))
            {
                Directory.Delete(filePath, true);
            }
            else if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            else
            {
                throw new InvalidOperationException(String.Format("Path '{0}' does not exist.", filePath));
            }
        }

        public void CreateFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                throw new InvalidOperationException("File already exists. You should be able to see it when you enable 'Show all files' option.");
            }
            File.Create(filePath).Dispose();
        }

        public void CreateDirectory(string filePath)
        {
            if (Directory.Exists(filePath))
            {
                throw new InvalidOperationException("Directory already exists. You should be able to see it when you enable 'Show all files' option.");
            }
            Directory.CreateDirectory(filePath);
        }

        public bool IsSubdirectory(string rootDir, string potentialSubDir)
        {
            if (String.IsNullOrEmpty(rootDir) || String.IsNullOrEmpty(potentialSubDir))
            {
                return false;
            }

            DirectoryInfo root = new DirectoryInfo(rootDir);
            DirectoryInfo sub = new DirectoryInfo(potentialSubDir);
            while (sub.Parent != null)
            {
                if (sub.Parent.FullName == root.FullName)
                {
                    return true;
                }
                sub = sub.Parent;
            }
            return false;
        }

    }
}
