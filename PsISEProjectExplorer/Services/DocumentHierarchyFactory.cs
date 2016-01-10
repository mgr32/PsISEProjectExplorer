using PsISEProjectExplorer.Config;
using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.Model.DocHierarchy;
using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using PsISEProjectExplorer.UI.Workers;
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

        public DocumentHierarchySearcher CreateDocumentHierarchySearcher(string path, bool analyzeContents)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            this.DocumentHierarchy = new DocumentHierarchy(new RootNode(path), analyzeContents);
            return new DocumentHierarchySearcher(this.DocumentHierarchy);           
        }

        public INode CreateTemporaryNode(INode parent, NodeType nodeType)
        {
            if (this.DocumentHierarchy == null || parent == null)
            {
                return null;
            }
            if (nodeType == NodeType.Directory)
            {
                return this.DocumentHierarchy.CreateNewDirectoryNode(parent.Path + @"\", parent, false, null);
            }
            if (nodeType == NodeType.File)
            {
                return this.DocumentHierarchy.CreateNewFileNode(parent.Path + @"\", string.Empty, parent, false, null);
            }
            return null;
        }

        public INode UpdateTemporaryNode(INode node, string newPath)
        {
            if (this.DocumentHierarchy == null || node == null)
            {
                return null;
            }
            if (node.NodeType == NodeType.Directory)
            {
                return this.DocumentHierarchy.UpdateDirectoryNodePath(node, newPath, null);
            }
            if (node.NodeType == NodeType.File)
            {
                return this.DocumentHierarchy.UpdateFileNodePath(node, newPath, null);
            }
            return null;
        }

        public void RemoveTemporaryNode(INode node)
        {
            if (this.DocumentHierarchy == null)
            {
                return;
            }
            this.DocumentHierarchy.RemoveNode(node);
        }

        public bool UpdateDocumentHierarchy(IEnumerable<string> pathsToUpdate, IEnumerable<string> excludePaths, FilesPatternProvider filesPatternProvider, BackgroundIndexer worker)
        {

            if (this.DocumentHierarchy == null)
            {
                return false;
            }
            var documentHierarchyIndexer = new DocumentHierarchyIndexer(this.DocumentHierarchy);
            bool changed = false;
            foreach (string path in pathsToUpdate)
            {
                INode node = this.DocumentHierarchy.GetNode(path);
                bool nodeShouldBeRemoved = node != null;
                var fileSystemEntryList = this.GetFileList(path, excludePaths, filesPatternProvider, worker);
                
                foreach (PowershellFileParser fileSystemEntry in fileSystemEntryList)
                {
                    // this is to prevent from reporting progress after deletion if the node is only updated
                    if (fileSystemEntry.Path == path && node != null)
                    {
                        this.DocumentHierarchy.RemoveNode(node);
                        nodeShouldBeRemoved = false;
                    }
                    documentHierarchyIndexer.AddFileSystemNode(fileSystemEntry);
                    changed = true;
                }
                if (nodeShouldBeRemoved)
                {
                    this.DocumentHierarchy.RemoveNode(node);
                    changed = true;
                    this.ReportProgress(worker, path);
                }
            }
            return changed;
            
        }

        private void ReportProgress(BackgroundIndexer worker, string path)
        {
            if (worker.CancellationPending)
            {
                throw new OperationCanceledException();
            }
            worker.ReportProgressInCurrentThread(path);
        }

        private IEnumerable<PowershellFileParser> GetFileList(string path, IEnumerable<string> excludePaths, FilesPatternProvider filesPatternProvider, BackgroundIndexer worker)
        {
            PowershellFileParser parser = null;
            Queue<string> pathsToEnumerate = new Queue<string>();

            if (File.Exists(path) && filesPatternProvider.DoesFileMatch(path))
            {
                parser = new PowershellFileParser(path, isDirectory: false);
                yield return parser;
                this.ReportProgress(worker, path);
                yield break;
            }
            if (!Directory.Exists(path) || !filesPatternProvider.DoesDirectoryMatch(path))
            {
                yield break;
            }
            parser = new PowershellFileParser(path, isDirectory: true);
            yield return parser;
            pathsToEnumerate.Enqueue(path);
            while (pathsToEnumerate.Any())
            {
                IEnumerable<string> dirs = null;
                parser = null;
                string currentPath = pathsToEnumerate.Dequeue();

                foreach (var file in this.GetFilesInDirectory(currentPath, excludePaths, filesPatternProvider))
                {
                    yield return file;
                }

                try {
                    dirs = Directory.EnumerateDirectories(currentPath).Where(dir => filesPatternProvider.DoesDirectoryMatch(dir));
                } catch (Exception e) 
                {
                    parser = new PowershellFileParser(currentPath, isDirectory: true, errorMessage: e.Message);
                }
                if (parser != null)
                {
                    yield return parser;
                    continue;
                }
                foreach (string dir in dirs)
                {
                    bool isExcluded = excludePaths.Any(e => dir.StartsWith(e));
                    if (filesPatternProvider.DoesDirectoryMatch(dir) && (isExcluded || filesPatternProvider.IncludeAllFiles || filesPatternProvider.IsInAdditonalPaths(dir)))
                    {
                        parser = new PowershellFileParser(dir, isDirectory: true, isExcluded: isExcluded);
                        yield return parser;
                    }
                    if (!isExcluded)
                    {
                        pathsToEnumerate.Enqueue(dir);
                    }
                }
                this.ReportProgress(worker, currentPath);
            } while (pathsToEnumerate.Any());
        }

        private IEnumerable<PowershellFileParser> GetFilesInDirectory(string path, IEnumerable<string> excludePaths, FilesPatternProvider filesPatternProvider)
        {
            IEnumerable<string> files = null;
            PowershellFileParser parser = null;
            try
            {
                files = Directory.GetFiles(path, filesPatternProvider.GetFilesPattern()).Where(f => filesPatternProvider.DoesFileMatch(f));
            }
            catch (Exception e)
            {
                parser = new PowershellFileParser(path, isDirectory: true, errorMessage: e.Message);
            }

            if (parser != null)
            {
                yield return parser;
                yield break;
            }

            foreach (string file in files)
            {
                bool isExcluded = excludePaths.Contains(file);
                parser = new PowershellFileParser(file, isDirectory: false, isExcluded: isExcluded);
                yield return parser;
            }
        }
    }
}
