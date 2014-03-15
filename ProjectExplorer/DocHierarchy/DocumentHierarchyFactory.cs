using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectExplorer.DocHierarchy
{
    public class DocumentHierarchyFactory
    {
        private static string FILES_PATTERN = "*.ps*1";

        public DocumentHierarchy CreateDocumentHierarchy(string path)
        {
            DocumentHierarchy documentHierarchy = new DocumentHierarchy(path);
            IList<string> fileSystemEntryList = new List<string>();
            this.FillFileListRecursively(path, fileSystemEntryList);

            foreach (string fileSystemEntry in fileSystemEntryList) {
                documentHierarchy.AddFileSystemNode(fileSystemEntry);
            }
            return documentHierarchy;
        }

        private bool FillFileListRecursively(string path, IList<string> result)
        {           
            foreach (string dir in Directory.EnumerateDirectories(path))
            {
                if (this.FillFileListRecursively(dir, result))
                {
                    result.Add(dir);
                }
            }

            var files = Directory.EnumerateFiles(path, FILES_PATTERN);
            foreach (string file in files)
            {
                result.Add(file);
            }
            return files.Any();

        }

    }
}
