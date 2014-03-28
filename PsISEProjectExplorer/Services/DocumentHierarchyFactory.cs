using PsISEProjectExplorer.Model;
using PsISEProjectExplorer.Model.DocHierarchy;
using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.Services
{
    public class DocumentHierarchyFactory
    {

        private IDictionary<string, DocumentHierarchy> DocumentHierarchies { get; set; }

        public DocumentHierarchyFactory()
        {
            this.DocumentHierarchies = new Dictionary<string, DocumentHierarchy>();
        }

        public DocumentHierarchy CreateDocumentHierarchy(string path)
        {
            DocumentHierarchy docHierarchy;
            lock (this.DocumentHierarchies)
            {
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
        }

        public DocumentHierarchy GetDocumentHierarchy(string rootPath)
        {
            lock (this.DocumentHierarchies)
            {
                return this.DocumentHierarchies[rootPath];
            }
        }

        public bool UpdateDocumentHierarchy(DocumentHierarchy docHierarchy, IEnumerable<string> pathsToUpdate)
        {
            lock (docHierarchy.RootNode)
            {
                DocumentHierarchyIndexer documentHierarchyIndexer = new DocumentHierarchyIndexer(docHierarchy);
                IList<PowershellFileParser> fileSystemEntryList = new List<PowershellFileParser>();
                bool changed = false;

                foreach (string path in pathsToUpdate)
                {
                    INode node = docHierarchy.GetNode(path);
                    if (node != null)
                    {
                        docHierarchy.RemoveNode(node);
                        changed = true;
                    }
                    if (File.Exists(path) && FilesPatternProvider.POWERSHELL_FILES_REGEX.IsMatch(path))
                    {
                        fileSystemEntryList.Add(new PowershellFileParser(path, false));
                    }
                    else if (Directory.Exists(path))
                    {
                        this.FillFileListRecursively(path, fileSystemEntryList);
                    }
                }

                foreach (PowershellFileParser fileSystemEntry in fileSystemEntryList)
                {
                    documentHierarchyIndexer.AddFileSystemNode(fileSystemEntry);
                }
                if (fileSystemEntryList.Any())
                {
                    changed = true;
                }
                return changed;
            }
        }

        private bool FillFileListRecursively(string path, IList<PowershellFileParser> result)
        {           
            foreach (string dir in Directory.EnumerateDirectories(path))
            {
                if (this.FillFileListRecursively(dir, result))
                {
                    result.Add(new PowershellFileParser(dir, true));
                }
            }

            var files = Directory.EnumerateFiles(path, FilesPatternProvider.POWERSHELL_FILES_PATTERN);
            foreach (string file in files)
            {
                result.Add(new PowershellFileParser(file, false));
            }
            return files.Any();

        }

    }
}
