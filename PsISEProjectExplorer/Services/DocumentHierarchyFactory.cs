using PsISEProjectExplorer.Model.DocHierarchy;
using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.Services
{
    public class DocumentHierarchyFactory
    {
        private static string FILES_PATTERN = "*.ps*1";

        public DocumentHierarchy CreateDocumentHierarchy(string path)
        {
            DocumentHierarchy docHierarchy = new DocumentHierarchy(new RootNode(path));
            DocumentHierarchyIndexer documentHierarchyIndexer = new DocumentHierarchyIndexer(docHierarchy);
            IList<FileSystemParser> fileSystemEntryList = new List<FileSystemParser>();
            this.FillFileListRecursively(path, fileSystemEntryList);

            foreach (FileSystemParser fileSystemEntry in fileSystemEntryList)
            {
                documentHierarchyIndexer.AddFileSystemNode(fileSystemEntry);
            }
            return docHierarchy;
        }

        private bool FillFileListRecursively(string path, IList<FileSystemParser> result)
        {           
            foreach (string dir in Directory.EnumerateDirectories(path))
            {
                if (this.FillFileListRecursively(dir, result))
                {
                    result.Add(new FileSystemParser(dir, true));
                }
            }

            var files = Directory.EnumerateFiles(path, FILES_PATTERN);
            foreach (string file in files)
            {
                result.Add(new FileSystemParser(file, false));
            }
            return files.Any();

        }

    }
}
