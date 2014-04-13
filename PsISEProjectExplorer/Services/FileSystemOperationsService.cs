using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.Services
{
    public static class FileSystemOperationsService
    {
        public static void RenameFileOrDirectory(string filePath, string newFilePath)
        {
            if (newFilePath == filePath)
            {
                return;
            }
            if (Directory.Exists(filePath)) 
            {
                if (newFilePath.StartsWith(filePath))
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

        public static void DeleteFileOrDirectory(string filePath)
        {
            if (Directory.Exists(filePath))
            {
                Directory.Delete(filePath, true);
            }
            else if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public static void CreateFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                throw new InvalidOperationException("File already exists. You should be able to see it when you enable 'Show all files' option.");
            }
            File.Create(filePath).Dispose();
        }

        public static void CreateDirectory(string filePath)
        {
            if (Directory.Exists(filePath))
            {
                throw new InvalidOperationException("Directory already exists. You should be able to see it when you enable 'Show all files' option.");
            }
            Directory.CreateDirectory(filePath);
        }
    }
}
