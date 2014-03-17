using PsISEProjectExplorer.EnumsAndOptions;
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

        public void CutOffNode(INode node)
        {
            foreach (INode child in node.Children)
            {
                this.NodeMap.Remove(child.Path);
                this.FullTextDirectory.DeleteDocument(child.Path);
                this.CutOffNode(child);
            }
            node.CutOff();
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

        public IEnumerable<INode> SearchNodesFullText(string filter, FullTextFieldType fieldType)
        {
            IEnumerable<string> paths = this.FullTextDirectory.Search(filter, fieldType);
            return paths.Select(path => this.GetNode(path));
        }

        public IEnumerable<INode> SearchNodesByTerm(string filter, FullTextFieldType fieldType)
        {
            IEnumerable<string> paths = this.FullTextDirectory.SearchTerm(filter, fieldType);
            return paths.Select(path => this.GetNode(path));
        }

    }
}
