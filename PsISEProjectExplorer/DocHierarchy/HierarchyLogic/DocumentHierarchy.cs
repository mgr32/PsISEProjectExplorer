using ProjectExplorer.DocHierarchy.FullText;
using ProjectExplorer.DocHierarchy.Nodes;
using ProjectExplorer.EnumsAndOptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectExplorer.DocHierarchy.HierarchyLogic
{
    public class DocumentHierarchy
    {
        public INode RootNode { get; private set; }

        private FullTextDirectory FullTextDirectory { get; set; }

        private IDictionary<string, INode> NodeMap { get; set; }

        public DocumentHierarchy(string rootPath)
        {
            this.RootNode = new RootNode(rootPath);
            this.FullTextDirectory = new FullTextDirectory();
            this.NodeMap = new Dictionary<string, INode>();
            this.NodeMap.Add(rootPath, this.RootNode);
        }

        public bool AddFileSystemNode(FileSystemParser parser)
        {
            if (this.NodeMap.ContainsKey(parser.Path))
            {
                return false;
            }
            INode lastDirNode = this.FillHierarchyWithIntermediateDirectories(parser.Path, parser.IsDirectory);
            if (!parser.IsDirectory)
            {
                this.CreateNewFileNode(parser, lastDirNode);
            }
            return true;
        }

        public IEnumerable<INode> SearchNodesFullText(string filter, SearchOptions searchOptions)
        {
            IEnumerable<string> paths = this.FullTextDirectory.Search(filter, searchOptions.SearchField);
            var nodes = paths.Select(path => this.NodeMap[path]);
            return nodes;
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
                INode node = null;
                this.NodeMap.TryGetValue(currentAbsolutePath, out node);
                if (node == null)
                {
                    node = this.CreateNewIntermediateDirectoryNode(currentAbsolutePath, segment, currentNode);
                }
                currentNode = (INode)node;
            }
            return currentNode;
        }

        private INode CreateNewIntermediateDirectoryNode(string absolutePath, string segment, INode parent)
        {
            INode node = new DirectoryNode(absolutePath, segment, parent);
            this.NodeMap.Add(absolutePath, node);
            this.FullTextDirectory.DocumentCreator.AddDirectoryEntry(absolutePath, segment);
            return node;
        }

        private INode CreateNewFileNode(FileSystemParser parser, INode parent)
        {
            INode fileNode = new FileNode(parser.Path, parser.FileName, parent);
            this.NodeMap.Add(parser.Path, fileNode);
            this.FullTextDirectory.DocumentCreator.AddFileEntry(parser);
            foreach (PowershellFunction func in parser.PowershellFunctions)
            {
                INode functionNode = new PowershellFunctionNode(parser.Path, func, fileNode);
                this.NodeMap.Add(functionNode.Path, functionNode);
                this.FullTextDirectory.DocumentCreator.AddFunctionEntry(functionNode.Path, func.Name);
            }
            return fileNode;
        }
    }
}
