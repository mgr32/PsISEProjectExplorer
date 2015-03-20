using PsISEProjectExplorer.Model.DocHierarchy;
using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PsISEProjectExplorer.Services
{
	public class DocumentHierarchyIndexer
    {
        private DocumentHierarchy DocumentHierarchy { get; set; }

        private INode RootNode 
        {   get
            {
                return DocumentHierarchy.RootNode;
            }
        }

        public DocumentHierarchyIndexer(DocumentHierarchy documentHierarchy)
        {
			DocumentHierarchy = documentHierarchy;
            
        }

        public bool AddFileSystemNode(PowershellFileParser parser)
        {
            if (DocumentHierarchy.GetNode(parser.Path) != null || parser.Path == DocumentHierarchy.RootNode.Path)
            {
                return false;
            }
            INode lastDirNode = FillHierarchyWithIntermediateDirectories(parser.Path, parser.IsDirectory, parser.ErrorMessage);
            if (!parser.IsDirectory)
            {
                FileNode fileNode = DocumentHierarchy.CreateNewFileNode(parser.Path, parser.FileContents, lastDirNode, parser.ErrorMessage);
                if (parser.RootPowershellItem != null)
                {
					DocumentHierarchy.CreateNewPowershellItemNode(parser.Path, parser.RootPowershellItem, fileNode);
                    var parent = fileNode.Parent;
                    while (parent != null && parent is DirectoryNode)
                    {
                        if (parser.RootPowershellItem.ParsingErrors != null)
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

        private INode FillHierarchyWithIntermediateDirectories(string path, bool lastSegmentIsDirectory, string errorMessage)
        {
            IList<string> segments = path.Replace(RootNode.Path + "\\", "").Split('\\').ToList();
            var currentNode = RootNode;
            if (!lastSegmentIsDirectory)
            {
                if (segments.Count <= 1)
                {
                    return currentNode;
                }
                segments.RemoveAt(segments.Count - 1);
            }
            var currentAbsolutePath = RootNode.Path;
            foreach (string segment in segments)
            {
                currentAbsolutePath = Path.Combine(currentAbsolutePath, segment);
                currentNode = DocumentHierarchy.GetNode(currentAbsolutePath) ??
					DocumentHierarchy.CreateNewDirectoryNode(currentAbsolutePath, currentNode, currentAbsolutePath == path ? errorMessage : null);
            }
            return currentNode;
        }
    }
}
