using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.Model;
using PsISEProjectExplorer.Model.DocHierarchy;
using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace PsISEProjectExplorer.Services
{
    public class DocumentHierarchySearcher
    {
        private DocumentHierarchy DocumentHierarchy { get; set; }

        public INode RootNode
        {
            get
            {
                return this.DocumentHierarchy == null ? null : this.DocumentHierarchy.RootNode;
            }
        }

        public DocumentHierarchySearcher(DocumentHierarchy documentHierarchy)
        {
            this.DocumentHierarchy = documentHierarchy;
        }

        // note: can be invoked by multiple threads simultaneously
        public INode GetDocumentHierarchyViewNodeProjection(string path, string filter, SearchOptions searchOptions, BackgroundWorker worker)
        {
            if (this.DocumentHierarchy == null)
            {
                return null;
            }
            var node = path == null ? this.DocumentHierarchy.RootNode : this.DocumentHierarchy.GetNode(path);
            if (node == null || String.IsNullOrWhiteSpace(filter))
            {
                return node;
            }
            IList<INode> filteredNodes = this.DocumentHierarchy
                .SearchNodesFullText(filter, searchOptions.SearchField)
                .Where(result => result.Path.StartsWith(node.Path)) // TODO: filter it earlier for performance
                .Select(result => result.Node)
                .ToList();
            this.ReportProgress(worker);
            return this.FillNewFilteredDocumentHierarchyRecursively(filteredNodes, node, null, worker);
        }

        public INode GetFunctionNodeByName(string name)
        {
            if (this.DocumentHierarchy == null)
            {
                return null;
            }
            return this.DocumentHierarchy
                .SearchNodesByTerm(name, FullTextFieldType.NameNotAnalyzed)
                .Select(result => result.Node)
                .FirstOrDefault(node => node.NodeType == NodeType.Function);
        }

        private INode CreateNewViewNodeWithParents(INode node)
        {
            var parent = node.Parent == null ? null : this.CreateNewViewNodeWithParents(node.Parent);
            return new ViewNode(node, parent);
        }

        private INode FillNewFilteredDocumentHierarchyRecursively(ICollection<INode> filteredNodes, INode node, INode viewNodeParent, BackgroundWorker worker)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            if (!filteredNodes.Any(filteredNode => filteredNode.Path.StartsWith(node.Path)))
            {
                return null;
            }
            INode viewNode;
            if (viewNodeParent == null)
            {
                viewNode = this.CreateNewViewNodeWithParents(node);
            }
            else
            {
                viewNode = new ViewNode(node, viewNodeParent);
            }
            IEnumerable<INode> nodeChildren;
            lock (node)
            {
                nodeChildren = new List<INode>(node.Children);
            }
            foreach (INode child in nodeChildren)
            {
                this.FillNewFilteredDocumentHierarchyRecursively(filteredNodes, child, viewNode, worker);
            }
            this.ReportProgress(worker);
            return viewNode;          
        }

        private void ReportProgress(BackgroundWorker worker)
        {
            if (worker.CancellationPending)
            {
                throw new OperationCanceledException();
            }
        }
    }
}
