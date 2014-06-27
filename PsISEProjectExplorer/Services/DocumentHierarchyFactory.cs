using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.Model;
using PsISEProjectExplorer.Model.DocHierarchy;
using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public DocumentHierarchySearcher CreateDocumentHierarchySearcher(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            this.DocumentHierarchy = new DocumentHierarchy(new RootNode(path));
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

        public bool UpdateDocumentHierarchy(IEnumerable<string> pathsToUpdate, FilesPatternProvider filesPatternProvider, BackgroundWorker worker)
        {
            if (this.DocumentHierarchy == null)
            {
                return false;
            }
            lock (this.DocumentHierarchy.RootNode)
            {
                var documentHierarchyIndexer = new DocumentHierarchyIndexer(this.DocumentHierarchy);
                bool changed = false;
                foreach (string path in pathsToUpdate)
                {
                    INode node = this.DocumentHierarchy.GetNode(path);
                    if (node != null)
                    {
                        this.DocumentHierarchy.RemoveNode(node);
                        changed = true;
                    }
                    var fileSystemEntryList = this.GetFileList(path, filesPatternProvider, worker);
                    foreach (PowershellFileParser fileSystemEntry in fileSystemEntryList)
                    {
                        documentHierarchyIndexer.AddFileSystemNode(fileSystemEntry);
                        changed = true;
                    }    
                }
                return changed;
            }
        }

        private void ReportProgress(BackgroundWorker worker, string path)
        {
            if (worker.CancellationPending)
            {
                throw new OperationCanceledException();
            }
            worker.ReportProgress(0, path);
        }

        private IEnumerable<PowershellFileParser> GetFileList(string path, FilesPatternProvider filesPatternProvider, BackgroundWorker worker)
        {
            PowershellFileParser parser = null;
            Queue<string> pathsToEnumerate = new Queue<string>();

            if (File.Exists(path) && filesPatternProvider.DoesFileMatch(path))
            {
                parser = new PowershellFileParser(path, isDirectory: false);
                yield return parser;
            }
            if (!Directory.Exists(path) || !filesPatternProvider.DoesDirectoryMatch(path))
            {
                yield break;
            }
            pathsToEnumerate.Enqueue(path);
            while (pathsToEnumerate.Any())
            {
                IEnumerable<string> dirs = null;
                parser = null;
                string currentPath = pathsToEnumerate.Dequeue();

                this.ReportProgress(worker, currentPath);

                foreach (var file in this.GetFilesInDirectory(currentPath, filesPatternProvider))
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
                    pathsToEnumerate.Enqueue(dir);
                }
            } while (pathsToEnumerate.Any());
        }

    /*
        private void FillFileListRecursivelyRoot(string path, IList<PowershellFileParser> result, FilesPatternProvider filesPatternProvider, BackgroundWorker worker)
        {
            if (!filesPatternProvider.DoesDirectoryMatch(path))
            {
                return;
            }
            bool anyMatchingFilesInDir = this.FillFileListRecursively(path, result, filesPatternProvider, worker);
            if (filesPatternProvider.IncludeAllFiles || anyMatchingFilesInDir || filesPatternProvider.IsInAdditonalPaths(path))
            {
                result.Add(new PowershellFileParser(path, isDirectory: true));
            }
        }

        private bool FillFileListRecursively(string path, IList<PowershellFileParser> result, FilesPatternProvider filesPatternProvider, BackgroundWorker worker)
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

            this.ReportProgress(worker, new IndexingProgressInfo(IndexingProgressInfo.ProgressType.EnumeratingFiles, path));
            bool anyMatchingFiles = this.AddFilesInDirectory(path, result, filesPatternProvider);
            
            foreach (string dir in dirs)
            {
                if (!filesPatternProvider.DoesDirectoryMatch(dir))
                {
                    continue;
                }
                this.ReportProgress(worker, new IndexingProgressInfo(IndexingProgressInfo.ProgressType.EnumeratingDirectories, dir));
                var anyMatchingFilesInDir = this.FillFileListRecursively(dir, result, filesPatternProvider, worker);
                if (filesPatternProvider.DoesDirectoryMatch(dir) && (filesPatternProvider.IncludeAllFiles || anyMatchingFilesInDir || filesPatternProvider.IsInAdditonalPaths(dir)))
                {
                    result.Add(new PowershellFileParser(dir, isDirectory: true));
                }
            }

            return anyMatchingFiles;
        }
    */
        private IEnumerable<PowershellFileParser> GetFilesInDirectory(string path, FilesPatternProvider filesPatternProvider)
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
                parser = new PowershellFileParser(file, isDirectory: false);
                yield return parser;
            }
        }
    }
}
