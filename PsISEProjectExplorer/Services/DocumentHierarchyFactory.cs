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
                return DocumentHierarchy == null ? null : DocumentHierarchy.RootNode.Path;
            }
        }

        public DocumentHierarchySearcher CreateDocumentHierarchySearcher(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
			DocumentHierarchy = new DocumentHierarchy(new RootNode(path));
            return new DocumentHierarchySearcher(DocumentHierarchy);           
        }

        public INode CreateTemporaryNode(INode parent, NodeType nodeType)
        {
            if (DocumentHierarchy == null || parent == null)
            {
                return null;
            }
            if (nodeType == NodeType.Directory)
            {
                return DocumentHierarchy.CreateNewDirectoryNode(parent.Path + @"\", parent, null);
            }
            if (nodeType == NodeType.File)
            {
                return DocumentHierarchy.CreateNewFileNode(parent.Path + @"\", string.Empty, parent, null);
            }
            return null;
        }

        public INode UpdateTemporaryNode(INode node, string newPath)
        {
            if (DocumentHierarchy == null || node == null)
            {
                return null;
            }
            if (node.NodeType == NodeType.Directory)
            {
                return DocumentHierarchy.UpdateDirectoryNodePath(node, newPath, null);
            }
            if (node.NodeType == NodeType.File)
            {
                return DocumentHierarchy.UpdateFileNodePath(node, newPath, null);
            }
            return null;
        }

        public void RemoveTemporaryNode(INode node)
        {
            if (DocumentHierarchy == null)
            {
                return;
            }
			DocumentHierarchy.RemoveNode(node);
        }

        public bool UpdateDocumentHierarchy(IEnumerable<string> pathsToUpdate, FilesPatternProvider filesPatternProvider, BackgroundIndexer worker)
        {
            if (DocumentHierarchy == null)
            {
                return false;
            }
            var documentHierarchyIndexer = new DocumentHierarchyIndexer(DocumentHierarchy);
            bool changed = false;
            foreach (string path in pathsToUpdate)
            {
                INode node = DocumentHierarchy.GetNode(path);
                bool nodeShouldBeRemoved = node != null;
                var fileSystemEntryList = GetFileList(path, filesPatternProvider, worker);
                
                foreach (PowershellFileParser fileSystemEntry in fileSystemEntryList)
                {
                    // this is to prevent from reporting progress after deletion if the node is only updated
                    if (fileSystemEntry.Path == path && node != null)
                    {
						DocumentHierarchy.RemoveNode(node);
                        nodeShouldBeRemoved = false;
                    }
                    documentHierarchyIndexer.AddFileSystemNode(fileSystemEntry);
                    changed = true;
                }
                if (nodeShouldBeRemoved)
                {
					DocumentHierarchy.RemoveNode(node);
                    changed = true;
					ReportProgress(worker, path);
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

        private IEnumerable<PowershellFileParser> GetFileList(string path, FilesPatternProvider filesPatternProvider, BackgroundIndexer worker)
        {
            PowershellFileParser parser = null;
            Queue<string> pathsToEnumerate = new Queue<string>();

            if (File.Exists(path) && filesPatternProvider.DoesFileMatch(path))
            {
                parser = new PowershellFileParser(path, isDirectory: false);
                yield return parser;
				ReportProgress(worker, path);
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

                foreach (var file in GetFilesInDirectory(currentPath, filesPatternProvider))
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
                    if (filesPatternProvider.DoesDirectoryMatch(dir) && (filesPatternProvider.IncludeAllFiles || filesPatternProvider.IsInAdditonalPaths(dir)))
                    {
                        parser = new PowershellFileParser(dir, isDirectory: true);
                        yield return parser;
                    }
                    pathsToEnumerate.Enqueue(dir);
                }
				ReportProgress(worker, currentPath);
            } while (pathsToEnumerate.Any());
        }

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
