using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.FullText;
using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.Model.DocHierarchy
{
    public class DocumentHierarchy
    {
        public INode RootNode { get; private set; }

        private FullTextDirectory FullTextDirectory { get; set; }

        private IDictionary<string, INode> NodeMap { get; set; }

        public DocumentHierarchy(INode rootNode)
        {
            this.RootNode = rootNode;
            this.FullTextDirectory = new FullTextDirectory();
            this.NodeMap = new Dictionary<string, INode>();
            this.NodeMap.Add(rootNode.Path, rootNode);
        }

        public INode GetNode(string path)
        {
            INode value;
            this.NodeMap.TryGetValue(path, out value);
            return value;
        }

        public void RemoveNode(INode node)
        {
            this.NodeMap.Remove(node.Path);
            this.FullTextDirectory.DeleteDocument(node.Path);
            node.Remove();
            var itemsToBeRemoved = new List<INode>(node.Children);
            foreach (INode child in itemsToBeRemoved)
            {
                this.RemoveNode(child);
            }
        }


        public INode CreateNewIntermediateDirectoryNode(string absolutePath, string segment, INode parent)
        {
            INode node = new DirectoryNode(absolutePath, segment, parent);
            this.NodeMap.Add(absolutePath, node);
            this.FullTextDirectory.DocumentCreator.AddDirectoryEntry(absolutePath, segment);
            return node;
        }

        public INode CreateNewFileNode(string absolutePath, string fileName, string fileContents, INode parent)
        {
            INode fileNode = new FileNode(absolutePath, fileName, parent);
            this.NodeMap.Add(absolutePath, fileNode);
            this.FullTextDirectory.DocumentCreator.AddFileEntry(absolutePath, fileName, fileContents);
            return fileNode;
        }

        public INode CreateNewFunctionNode(string path, PowershellFunction func, INode parent)
        {
            INode functionNode = new PowershellFunctionNode(path, func, parent);
            this.NodeMap.Add(functionNode.Path, functionNode);
            this.FullTextDirectory.DocumentCreator.AddFunctionEntry(functionNode.Path, func.Name);
            return functionNode;
        }

        public IEnumerable<SearchResult> SearchNodesFullText(string filter, FullTextFieldType fieldType)
        {
            IEnumerable<SearchResult> searchResults = this.FullTextDirectory.Search(filter, fieldType);
            this.AddNodesToSearchResults(searchResults);
            return searchResults;
        }

        public IEnumerable<SearchResult> SearchNodesByTerm(string filter, FullTextFieldType fieldType)
        {
            IEnumerable<SearchResult> searchResults = this.FullTextDirectory.SearchTerm(filter, fieldType);
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
