using PsISEProjectExplorer.Model;
using PsISEProjectExplorer.Model.DocHierarchy;
using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PsISEProjectExplorer.Services
{
    [Component]
    public class DocumentHierarchyIndexer
    {

        public bool AddFileSystemNode(DocumentHierarchy documentHierarchy, PowershellParseResult parseResult)
        {
            if (documentHierarchy.GetNode(parseResult.Path) != null || parseResult.Path == documentHierarchy.RootNode.Path)
            {
                return false;
            }
            INode lastDirNode = this.FillHierarchyWithIntermediateDirectories(documentHierarchy, parseResult.Path, parseResult.IsDirectory, parseResult.IsExcluded, parseResult.ErrorMessage);
            if (!parseResult.IsDirectory)
            {
                FileNode fileNode = documentHierarchy.CreateNewFileNode(parseResult.Path, parseResult.FileContents, lastDirNode, parseResult.IsExcluded, parseResult.ErrorMessage);
                if (parseResult.RootPowershellItem != null)
                {
                    documentHierarchy.CreateNewPowershellItemNode(parseResult.Path, parseResult.RootPowershellItem, fileNode);
                    var parent = fileNode.Parent;
                    while (parent != null && parent is DirectoryNode)
                    {
                        if (parseResult.RootPowershellItem.ParsingErrors != null)
                        {
                            ((DirectoryNode)parent).AddFileError(fileNode.Name);
                        }
                        else
                        {
                            ((DirectoryNode)parent).RemoveFileError(fileNode.Name);
                        }
                        parent = parent.Parent;
                    }
                }
            }
            return true;
        }

        private INode FillHierarchyWithIntermediateDirectories(DocumentHierarchy documentHierarchy, string path, bool lastSegmentIsDirectory, bool isExcluded, string errorMessage)
        {
            IList<string> segments = path.Replace(documentHierarchy.RootNode.Path + "\\", "").Split('\\').ToList();
            var currentNode = documentHierarchy.RootNode;
            if (!lastSegmentIsDirectory)
            {
                if (segments.Count <= 1)
                {
                    return currentNode;
                }
                segments.RemoveAt(segments.Count - 1);
                isExcluded = false;
            }
            var currentAbsolutePath = documentHierarchy.RootNode.Path;
            int lastIndex = segments.Count - 1;
            int i = 0;
            foreach (string segment in segments)
            {
                currentAbsolutePath = Path.Combine(currentAbsolutePath, segment);
                bool nodeIsExcluded = i == lastIndex ? isExcluded : false;
                currentNode = documentHierarchy.GetNode(currentAbsolutePath) ??
                    documentHierarchy.CreateNewDirectoryNode(currentAbsolutePath, currentNode, nodeIsExcluded, currentAbsolutePath == path ? errorMessage : null);
                i++;
            }
            return currentNode;
        }
    }
}
