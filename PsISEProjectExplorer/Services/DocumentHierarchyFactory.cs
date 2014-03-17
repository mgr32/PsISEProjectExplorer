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
            this.UpdateDocumentHierarchy(docHierarchy, path);
            return docHierarchy;
        }

        public DocumentHierarchy UpdateDocumentHierarchy(string rootPath, string pathToUpdate)
        {
            DocumentHierarchy docHierarchy = this.DocumentHierarchies[rootPath];
            this.UpdateDocumentHierarchy(docHierarchy, pathToUpdate);
            return docHierarchy;
        }

        private void UpdateDocumentHierarchy(DocumentHierarchy docHierarchy, string pathToUpdate)
        {
            DocumentHierarchyIndexer documentHierarchyIndexer = new DocumentHierarchyIndexer(docHierarchy);
            IList<FileSystemParser> fileSystemEntryList = new List<FileSystemParser>();
            this.FillFileListRecursively(pathToUpdate, fileSystemEntryList);

            INode node = docHierarchy.GetNode(pathToUpdate);
            docHierarchy.CutOffNode(node);
            foreach (FileSystemParser fileSystemEntry in fileSystemEntryList)
            {
                documentHierarchyIndexer.AddFileSystemNode(fileSystemEntry);
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
