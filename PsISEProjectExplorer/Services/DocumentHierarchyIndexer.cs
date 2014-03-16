using PsISEProjectExplorer.EnumsAndOptions;
using PsISEProjectExplorer.FullText;
using PsISEProjectExplorer.Model;
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

        public bool AddFileSystemNode(FileSystemParser parser)
        {
            if (this.DocumentHierarchy.GetNode(parser.Path) != null)
            {
                return false;
            }
            INode lastDirNode = this.FillHierarchyWithIntermediateDirectories(parser.Path, parser.IsDirectory);
            if (!parser.IsDirectory)
            {
                INode fileNode = this.DocumentHierarchy.CreateNewFileNode(parser.Path, parser.FileName, parser.FileContents, lastDirNode);
                foreach (PowershellFunction func in parser.PowershellFunctions)
                {
                    this.DocumentHierarchy.CreateNewFunctionNode(parser.Path, func, fileNode);
                }
            }
            return true;
        }

        private INode FillHierarchyWithIntermediateDirectories(string path, bool lastSegmentIsDirectory)
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
                INode node = this.DocumentHierarchy.GetNode(currentAbsolutePath);
                if (node == null)
                {
                    node = this.DocumentHierarchy.CreateNewIntermediateDirectoryNode(currentAbsolutePath, segment, currentNode);
                }
                currentNode = (INode)node;
            }
            return currentNode;
        }

        
    }
}
