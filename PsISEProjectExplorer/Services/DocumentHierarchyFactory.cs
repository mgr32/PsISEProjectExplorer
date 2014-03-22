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

        private IDictionary<string, DocumentHierarchy> DocumentHierarchies { get; set; }

        public DocumentHierarchyFactory()
        {
            this.DocumentHierarchies = new Dictionary<string, DocumentHierarchy>();
        }

        public DocumentHierarchy CreateDocumentHierarchy(string path)
        {
            DocumentHierarchy docHierarchy;
            this.DocumentHierarchies.TryGetValue(path, out docHierarchy);
            if (docHierarchy != null)
            {
                return docHierarchy;
            }
            docHierarchy = new DocumentHierarchy(new RootNode(path));
            this.DocumentHierarchies.Add(path, docHierarchy);
            this.UpdateDocumentHierarchy(docHierarchy, new List<string>() { path });
            return docHierarchy;
        }

        public DocumentHierarchy UpdateDocumentHierarchy(string rootPath, IEnumerable<string> pathsToUpdate)
        {
            DocumentHierarchy docHierarchy = this.DocumentHierarchies[rootPath];
            this.UpdateDocumentHierarchy(docHierarchy, pathsToUpdate);
            return docHierarchy;
        }

        private void UpdateDocumentHierarchy(DocumentHierarchy docHierarchy, IEnumerable<string> pathsToUpdate)
        {
            lock (docHierarchy)
            {
                DocumentHierarchyIndexer documentHierarchyIndexer = new DocumentHierarchyIndexer(docHierarchy);
                IList<FileSystemParser> fileSystemEntryList = new List<FileSystemParser>();

                foreach (string path in pathsToUpdate)
                {
                    INode node = docHierarchy.GetNode(path);
                    if (node != null)
                    {
                        docHierarchy.RemoveNode(node);
                    }
                    // TODO: check if still matches pattern
                    if (File.Exists(path))
                    {
                        fileSystemEntryList.Add(new FileSystemParser(path, false));
                    }
                    else if (Directory.Exists(path))
                    {
                        this.FillFileListRecursively(path, fileSystemEntryList);
                    }
                }

                foreach (FileSystemParser fileSystemEntry in fileSystemEntryList)
                {
                    documentHierarchyIndexer.AddFileSystemNode(fileSystemEntry);
                }
            }
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
