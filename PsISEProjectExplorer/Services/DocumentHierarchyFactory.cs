using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.Model;
using PsISEProjectExplorer.Model.DocHierarchy;
using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PsISEProjectExplorer.Services
{
    public class DocumentHierarchyFactory
    {

        private DocumentHierarchy DocumentHierarchy { get; set; }

        public string CurrentDocumentHierarchyPath
        {
            get
            {
                return this.DocumentHierarchy == null ? null : this.DocumentHierarchy.RootNode.Path;
            }
        }

        public DocumentHierarchySearcher CreateDocumentHierarchy(string path, FilesPatternProvider filesPatternProvider)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            this.DocumentHierarchy = new DocumentHierarchy(new RootNode(path));
            this.UpdateDocumentHierarchy(new List<string> { path }, filesPatternProvider);
            return new DocumentHierarchySearcher(this.DocumentHierarchy);
        }

        public INode CreateTemporaryNode(INode parent, NodeType nodeType)
        {
            if (this.DocumentHierarchy == null || parent == null)
            {
                return null;
            }
            lock (this.DocumentHierarchy.RootNode)
            {
                if (nodeType == NodeType.Directory)
                {
                    return this.DocumentHierarchy.CreateNewDirectoryNode(parent.Path + @"\", parent, null);
                }
                if (nodeType == NodeType.File)
                {
                    return this.DocumentHierarchy.CreateNewFileNode(parent.Path + @"\", string.Empty, parent, null);
                }
            }
            return null;
        }

        public INode UpdateTemporaryNode(INode node, string newPath)
        {
            if (this.DocumentHierarchy == null)
            {
                return null;
            }
            lock (this.DocumentHierarchy.RootNode)
            {
                if (node.NodeType == NodeType.Directory)
                {
                    return this.DocumentHierarchy.UpdateDirectoryNodePath(node, newPath, null);
                }
                if (node.NodeType == NodeType.File)
                {
                    return this.DocumentHierarchy.UpdateFileNodePath(node, newPath, null);
                }
            }
            return null;
        }

        public void RemoveTemporaryNode(INode node)
        {
            if (this.DocumentHierarchy == null)
            {
                return;
            }
            lock (this.DocumentHierarchy.RootNode)
            {
                this.DocumentHierarchy.RemoveNode(node);
            }
        }

        public DocumentHierarchySearcher UpdateDocumentHierarchy(IEnumerable<string> pathsToUpdate, FilesPatternProvider filesPatternProvider)
        {
            lock (this.DocumentHierarchy.RootNode)
            {
                var documentHierarchyIndexer = new DocumentHierarchyIndexer(this.DocumentHierarchy);
                IList<PowershellFileParser> fileSystemEntryList = new List<PowershellFileParser>();
                bool changed = false;

                foreach (string path in pathsToUpdate)
                {
                    INode node = this.DocumentHierarchy.GetNode(path);
                    if (node != null)
                    {
                        this.DocumentHierarchy.RemoveNode(node);
                        changed = true;
                    }
                    if (File.Exists(path) && filesPatternProvider.DoesFileMatch(path))
                    {
                        fileSystemEntryList.Add(new PowershellFileParser(path, isDirectory: false));
                    }
                    else if (Directory.Exists(path))
                    {
                        this.FillFileListRecursivelyRoot(path, fileSystemEntryList, filesPatternProvider);
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
                return changed ? new DocumentHierarchySearcher(this.DocumentHierarchy) : null;
            }
        }

        private void FillFileListRecursivelyRoot(string path, IList<PowershellFileParser> result, FilesPatternProvider filesPatternProvider)
        {
            if (!filesPatternProvider.DoesDirectoryMatch(path))
            {
                return;
            }
            bool anyMatchingFilesInDir = this.FillFileListRecursively(path, result, filesPatternProvider);
            if (filesPatternProvider.IncludeAllFiles || anyMatchingFilesInDir || filesPatternProvider.IsInAdditonalPaths(path))
            {
                result.Add(new PowershellFileParser(path, isDirectory: true));
            }
        }

        private bool FillFileListRecursively(string path, IList<PowershellFileParser> result, FilesPatternProvider filesPatternProvider)
        {
            IEnumerable<string> dirs = null;

            try {
                dirs = Directory.EnumerateDirectories(path);
            } catch (Exception e) 
            {
                if (filesPatternProvider.DoesDirectoryMatch(path))
                {
                    result.Add(new PowershellFileParser(path, isDirectory: true, errorMessage: e.Message));
                }
                return false;
            }
            foreach (string dir in dirs)
            {
                if (!filesPatternProvider.DoesDirectoryMatch(dir))
                {
                    continue;
                }
                var anyMatchingFilesInDir = this.FillFileListRecursively(dir, result, filesPatternProvider);
                if (filesPatternProvider.DoesDirectoryMatch(dir) && (filesPatternProvider.IncludeAllFiles || anyMatchingFilesInDir || filesPatternProvider.IsInAdditonalPaths(dir)))
                {
                    result.Add(new PowershellFileParser(dir, isDirectory: true));
                }
            }
            return this.AddFilesInDirectory(path, result, filesPatternProvider);
        }

        private bool AddFilesInDirectory(string path, IList<PowershellFileParser> result, FilesPatternProvider filesPatternProvider)
        {
            IEnumerable<string> files = null;
            try
            {
                files = Directory.GetFiles(path, filesPatternProvider.GetFilesPattern());
            }
            catch (Exception e)
            {
                var entry = result.FirstOrDefault(pfp => pfp.Path == path);
                if (entry == null)
                {
                    entry = new PowershellFileParser(path, isDirectory: true);
                }
                entry.ErrorMessage = e.Message;
            }

            foreach (string file in files.Where(f => filesPatternProvider.DoesFileMatch(f)))
            {
                result.Add(new PowershellFileParser(file, isDirectory: false));
            }
            return files.Any();

        }

    }
}
