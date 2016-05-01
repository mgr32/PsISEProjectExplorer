using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.FullText;
using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using System;
using System.Collections.Generic;
using System.IO;

namespace PsISEProjectExplorer.Model.DocHierarchy
{
    public class DocumentHierarchy
    {
        public static object RootLockObject = new object();

        private INode rootNode;

        public INode RootNode { get { return this.rootNode; } }

        private readonly FullTextDirectory fullTextDirectory;

        private readonly IDictionary<string, INode> nodeMap;

        public DocumentHierarchy(INode rootNode, bool analyzeContents)
        {
            this.rootNode = rootNode;
            this.fullTextDirectory = new FullTextDirectory(analyzeContents);
            this.nodeMap = new Dictionary<string, INode> {{rootNode.Path, rootNode}};
        }

        public INode GetNode(string path)
        {
            INode value;
            this.nodeMap.TryGetValue(path, out value);
            return value;
        }

        public void RemoveNode(INode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            var lockObject = node.Parent == null ? RootLockObject : node.Parent;
            IList<INode> itemsToBeRemoved;
            lock (lockObject)
            {
                if (node.Path != this.RootNode.Path)
                {
                    this.nodeMap.Remove(node.Path);
                    this.fullTextDirectory.DeleteDocument(node.Path);
                    node.Remove();
                }
                itemsToBeRemoved = new List<INode>(node.Children);
            }
            foreach (INode child in itemsToBeRemoved)
            {
                this.RemoveNode(child);
            }
        }


        public INode CreateNewDirectoryNode(string absolutePath, INode parent, bool isExcluded, string errorMessage)
        {
            var lockObject = parent == null ? RootLockObject : parent;
            lock (lockObject)
            {
                string name = absolutePath.Substring(absolutePath.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                INode node;
                node = new DirectoryNode(absolutePath, name, parent, isExcluded, errorMessage);
                this.nodeMap.Add(absolutePath, node);
                this.fullTextDirectory.DocumentFactory.AddDirectoryEntry(absolutePath, name);
                return node;
            }
        }

        public FileNode CreateNewFileNode(string absolutePath, string fileContents, INode parent, bool isExcluded, string errorMessage)
        {
            var lockObject = parent == null ? RootLockObject : parent;
            lock (lockObject)
            {
                string fileName = Path.GetFileName(absolutePath);
                FileNode fileNode = new FileNode(absolutePath, fileName, parent, isExcluded, errorMessage);
                this.nodeMap.Add(absolutePath, fileNode);
                this.fullTextDirectory.DocumentFactory.AddFileEntry(absolutePath, fileName, fileContents);
                return fileNode;
            }
        }

        public INode CreateNewPowershellItemNode(string filePath, PowershellItem item, INode parent)
        {
            if (item.Type != PowershellItemType.Root)
            {
                var lockObject = parent == null ? RootLockObject : parent;
                lock (lockObject)
                {
                    var newNode = new PowershellItemNode(filePath, item, parent);
                    this.nodeMap.Add(newNode.Path, newNode);
                    this.fullTextDirectory.DocumentFactory.AddPowershellItemEntry(newNode.Path, item.Name);
                    parent = newNode;
                }
            }
            foreach (var itemChild in item.Children)
            {
                this.CreateNewPowershellItemNode(filePath, itemChild, parent);
            }
            return parent;
        }

        public INode UpdateDirectoryNodePath(INode node, string newPath, string errorMessage)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            var lockObject = node.Parent == null ? RootLockObject : node.Parent;
            lock (lockObject)
            {
                this.RemoveNode(node);
                return this.CreateNewDirectoryNode(newPath, node.Parent, node.IsExcluded, errorMessage);
            }
        }

        public INode UpdateFileNodePath(INode node, string newPath, string errorMessage)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            var lockObject = node.Parent == null ? RootLockObject : node.Parent;
            lock (lockObject)
            {
                this.RemoveNode(node);
                return this.CreateNewFileNode(newPath, string.Empty, node.Parent, node.IsExcluded, errorMessage);
            }
        }

        public IEnumerable<SearchResult> SearchNodesFullText(SearchOptions searchOptions)
        {
            IList<SearchResult> searchResults = this.fullTextDirectory.Search(searchOptions);
            this.AddNodesToSearchResults(searchResults);
            return searchResults;
        }

        public IEnumerable<SearchResult> SearchNodesByTerm(string filter, FullTextFieldType fieldType)
        {
            IList<SearchResult> searchResults = this.fullTextDirectory.SearchTerm(filter, fieldType);
            this.AddNodesToSearchResults(searchResults);
            return searchResults;
        }

        private void AddNodesToSearchResults(IEnumerable<SearchResult> searchResults)
        {
            foreach (SearchResult searchResult in searchResults)
            {
                searchResult.Node = this.GetNode(searchResult.Path);
            }
        }

    }
}
