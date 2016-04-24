using PsISEProjectExplorer.Config;
using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.Model;
using PsISEProjectExplorer.Model.DocHierarchy;
using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using PsISEProjectExplorer.UI.Workers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PsISEProjectExplorer.Services
{
    [Component]
    public class DocumentHierarchyFactory
    {
        public DocumentHierarchy DocumentHierarchy { get; private set; }

        private PowershellFileParser PowershellFileParser { get; set; }

        private DocumentHierarchyIndexer DocumentHierarchyIndexer { get; set; }

        public string CurrentDocumentHierarchyPath
        {
            get
            {
                return this.DocumentHierarchy == null ? null : this.DocumentHierarchy.RootNode.Path;
            }
        }

        public DocumentHierarchyFactory(PowershellFileParser powershellFileParser, DocumentHierarchyIndexer documentHierarchyIndexer)
        {
            this.PowershellFileParser = powershellFileParser;
            this.DocumentHierarchyIndexer = documentHierarchyIndexer;
        }

        public DocumentHierarchy CreateDocumentHierarchy(string path, bool analyzeContents)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            this.DocumentHierarchy = new DocumentHierarchy(new RootNode(path), analyzeContents);
            return this.DocumentHierarchy;
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

        public bool UpdateDocumentHierarchy(IEnumerable<string> pathsToUpdate, FilesPatternProvider filesPatternProvider, BackgroundIndexer worker)
        {

            if (this.DocumentHierarchy == null)
            {
                return false;
            }
            bool changed = false;
            foreach (string path in pathsToUpdate)
            {
                INode node = this.DocumentHierarchy.GetNode(path);
                bool nodeShouldBeRemoved = node != null;
                var fileSystemEntryList = this.GetFileList(path, filesPatternProvider, worker);
                
                foreach (PowershellParseResult fileSystemEntry in fileSystemEntryList)
                {
                    // this is to prevent from reporting progress after deletion if the node is only updated
                    if (fileSystemEntry.Path == path && node != null)
                    {
                        this.DocumentHierarchy.RemoveNode(node);
                        nodeShouldBeRemoved = false;
                    }
                    this.DocumentHierarchyIndexer.AddFileSystemNode(this.DocumentHierarchy, fileSystemEntry);
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

        private IEnumerable<PowershellParseResult> GetFileList(string path, FilesPatternProvider filesPatternProvider, BackgroundIndexer worker)
        {
            PowershellParseResult parseResult = null;
            Queue<string> pathsToEnumerate = new Queue<string>();

            if (File.Exists(path) && filesPatternProvider.DoesFileMatch(path))
            {
                parseResult = this.PowershellFileParser.ParseFile(path, isDirectory: false, isExcluded: false, errorMessage: null);
                yield return parseResult;
                this.ReportProgress(worker, path);
                yield break;
            }
            if (!Directory.Exists(path) || !filesPatternProvider.DoesDirectoryMatch(path))
            {
                yield break;
            }
            parseResult = this.PowershellFileParser.ParseFile(path, isDirectory: true, isExcluded: false, errorMessage: null);
            yield return parseResult;
            pathsToEnumerate.Enqueue(path);
            while (pathsToEnumerate.Any())
            {
                IEnumerable<string> dirs = null;
                parseResult = null;
                string currentPath = pathsToEnumerate.Dequeue();

                foreach (var file in this.GetFilesInDirectory(currentPath, filesPatternProvider))
                {
                    yield return file;
                }

                try {
                    dirs = Directory.EnumerateDirectories(currentPath).Where(dir => filesPatternProvider.DoesDirectoryMatch(dir));
                } catch (Exception e) 
                {
                    parseResult = this.PowershellFileParser.ParseFile(currentPath, isDirectory: true, isExcluded: false, errorMessage: e.Message);
                }
                if (parseResult != null)
                {
                    yield return parseResult;
                    continue;
                }
                foreach (string dir in dirs)
                {
                    bool isExcluded = filesPatternProvider.IsExcluded(dir);
                    if (filesPatternProvider.DoesDirectoryMatch(dir) && (isExcluded || filesPatternProvider.IncludeAllFiles || filesPatternProvider.IsInAdditonalPaths(dir)))
                    {
                        parseResult = this.PowershellFileParser.ParseFile(dir, isDirectory: true, isExcluded: isExcluded, errorMessage: null);
                        yield return parseResult;
                    }
                    if (!isExcluded)
                    {
                        pathsToEnumerate.Enqueue(dir);
                    }
                }
                this.ReportProgress(worker, currentPath);
            } while (pathsToEnumerate.Any());
        }

        private IEnumerable<PowershellParseResult> GetFilesInDirectory(string path, FilesPatternProvider filesPatternProvider)
        {
            IEnumerable<string> files = null;
            PowershellParseResult parseResult = null;
            try
            {
                files = Directory.GetFiles(path, filesPatternProvider.GetFilesPattern()).Where(f => filesPatternProvider.DoesFileMatch(f));
            }
            catch (Exception e)
            {
                parseResult = this.PowershellFileParser.ParseFile(path, isDirectory: true, isExcluded: false, errorMessage: e.Message);
            }

            if (parseResult != null)
            {
                yield return parseResult;
                yield break;
            }

            foreach (string file in files)
            {
                parseResult = this.PowershellFileParser.ParseFile(file, isDirectory: false, isExcluded: filesPatternProvider.IsExcluded(file), errorMessage: null);
                yield return parseResult;
            }
        }
    }
}
