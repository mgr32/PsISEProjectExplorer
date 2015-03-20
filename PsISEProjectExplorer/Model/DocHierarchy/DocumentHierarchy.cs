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

        public INode RootNode { get; private set; }

        private FullTextDirectory FullTextDirectory { get; set; }

        private IDictionary<string, INode> NodeMap { get; set; }

        public DocumentHierarchy(INode rootNode)
        {
			RootNode = rootNode;
			FullTextDirectory = new FullTextDirectory();
			NodeMap = new Dictionary<string, INode> {{rootNode.Path, rootNode}};
        }

        public INode GetNode(string path)
        {
            INode value;
			NodeMap.TryGetValue(path, out value);
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
                if (node.Path != RootNode.Path)
                {
					NodeMap.Remove(node.Path);
					FullTextDirectory.DeleteDocument(node.Path);
                    node.Remove();
                }
                itemsToBeRemoved = new List<INode>(node.Children);
            }
            foreach (INode child in itemsToBeRemoved)
            {
				RemoveNode(child);
            }
        }


        public INode CreateNewDirectoryNode(string absolutePath, INode parent, string errorMessage)
        {
            var lockObject = parent == null ? RootLockObject : parent;
            lock (lockObject)
            {
                string name = absolutePath.Substring(absolutePath.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                INode node;
                node = new DirectoryNode(absolutePath, name, parent, errorMessage);
				NodeMap.Add(absolutePath, node);
				FullTextDirectory.DocumentCreator.AddDirectoryEntry(absolutePath, name);
                return node;
            }
        }

        public FileNode CreateNewFileNode(string absolutePath, string fileContents, INode parent, string errorMessage)
        {
            var lockObject = parent == null ? RootLockObject : parent;
            lock (lockObject)
            {
                string fileName = Path.GetFileName(absolutePath);
                FileNode fileNode = new FileNode(absolutePath, fileName, parent, errorMessage);
				NodeMap.Add(absolutePath, fileNode);
				FullTextDirectory.DocumentCreator.AddFileEntry(absolutePath, fileName, fileContents);
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
                    parent = new PowershellItemNode(filePath, item, parent);
					NodeMap.Add(parent.Path, parent);
					FullTextDirectory.DocumentCreator.AddPowershellItemEntry(parent.Path, item.Name);
                }
            }
            foreach (var itemChild in item.Children)
            {
				CreateNewPowershellItemNode(filePath, itemChild, parent);
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
				RemoveNode(node);
                return CreateNewDirectoryNode(newPath, node.Parent, errorMessage);
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
				RemoveNode(node);
                return CreateNewFileNode(newPath, string.Empty, node.Parent, errorMessage);
            }
        }

        public IEnumerable<SearchResult> SearchNodesFullText(string filter, FullTextFieldType fieldType)
        {
            IList<SearchResult> searchResults = FullTextDirectory.Search(filter, fieldType);
			AddNodesToSearchResults(searchResults);
            return searchResults;
        }

        public IEnumerable<SearchResult> SearchNodesByTerm(string filter, FullTextFieldType fieldType)
        {
            IList<SearchResult> searchResults = FullTextDirectory.SearchTerm(filter, fieldType);
			AddNodesToSearchResults(searchResults);
            return searchResults;
        }

        private void AddNodesToSearchResults(IEnumerable<SearchResult> searchResults)
        {
            foreach (SearchResult searchResult in searchResults)
            {
                searchResult.Node = GetNode(searchResult.Path);
            }
        }

    }
}
