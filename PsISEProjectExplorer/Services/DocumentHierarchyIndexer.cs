using PsISEProjectExplorer.Model;
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
                return this.DocumentHierarchy.RootNode;
            }
        }

        public DocumentHierarchyIndexer(DocumentHierarchy documentHierarchy)
        {
            this.DocumentHierarchy = documentHierarchy;
            
        }

        public bool AddFileSystemNode(PowershellFileParser parser)
        {
            if (this.DocumentHierarchy.GetNode(parser.Path) != null || parser.Path == this.DocumentHierarchy.RootNode.Path)
            {
                return false;
            }
            INode lastDirNode = this.FillHierarchyWithIntermediateDirectories(parser.Path, parser.IsDirectory, parser.ErrorMessage);
            if (!parser.IsDirectory)
            {
                INode fileNode = this.DocumentHierarchy.CreateNewFileNode(parser.Path, parser.FileContents, lastDirNode, parser.ErrorMessage);
                if (parser.RootPowershellItem != null)
                {
                    this.DocumentHierarchy.CreateNewPowershellItemNode(parser.Path, parser.RootPowershellItem, fileNode);
                }
            }
            return true;
        }

        private INode FillHierarchyWithIntermediateDirectories(string path, bool lastSegmentIsDirectory, string errorMessage)
        {
            IList<string> segments = path.Replace(this.RootNode.Path + "\\", "").Split('\\').ToList();
            var currentNode = this.RootNode;
            if (!lastSegmentIsDirectory)
            {
                if (segments.Count <= 1)
                {
                    return currentNode;
                }
                segments.RemoveAt(segments.Count - 1);
            }
            var currentAbsolutePath = this.RootNode.Path;
            foreach (string segment in segments)
            {
                currentAbsolutePath = Path.Combine(currentAbsolutePath, segment);
                currentNode = this.DocumentHierarchy.GetNode(currentAbsolutePath) ??
                    this.DocumentHierarchy.CreateNewDirectoryNode(currentAbsolutePath, currentNode, currentAbsolutePath == path ? errorMessage : null);
            }
            return currentNode;
        }
    }
}
