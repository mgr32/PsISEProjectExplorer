using ProjectExplorer.DocHierarchy.FullText;
using ProjectExplorer.DocHierarchy.Nodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectExplorer.DocHierarchy
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

        public bool AddFileSystemNode(string absolutePath)
        {
            if (this.NodeMap.ContainsKey(absolutePath))
            {
                return false;
            }
            
            string[] segments = absolutePath.Replace(this.RootNode.Path + "\\", "").Split('\\');
            var currentNode = this.RootNode;
            var currentAbsolutePath = this.RootNode.Path;
            foreach (string segment in segments)
            {
                currentAbsolutePath = Path.Combine(currentAbsolutePath, segment);
                INode node = null;
                this.NodeMap.TryGetValue(currentAbsolutePath, out node);
                if (node == null) {
                    node = FileSystemNode.CreateDirectoryOrFileNode(currentAbsolutePath, segment, currentNode);
                    this.NodeMap.Add(currentAbsolutePath, node);
                    this.FullTextDirectory.AddFileSystemEntry(currentAbsolutePath, segment);

                }
                currentNode = (FileSystemNode)node;
            }
            return true;
        }

        public IEnumerable<INode> SearchNodesFullText(string filter)
        {
            IEnumerable<string> paths = this.FullTextDirectory.Search(filter);
            var nodes = paths.Select(path => this.NodeMap[path]);
            return nodes;
        }



    }
}
