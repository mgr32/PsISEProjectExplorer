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
                Directory.Delete(filePath);
            }
            else if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public static void CreateFile(string filePath)
        {
            File.Create(filePath).Dispose();
        }

        public static void CreateDirectory(string filePath)
        {
            Directory.CreateDirectory(filePath);
        }
    }
}
