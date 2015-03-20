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
                return DocumentHierarchy == null ? null : DocumentHierarchy.RootNode;
            }
        }

        public DocumentHierarchySearcher(DocumentHierarchy documentHierarchy)
        {
			DocumentHierarchy = documentHierarchy;
        }

        // note: can be invoked by multiple threads simultaneously
        public INode GetDocumentHierarchyViewNodeProjection(string path, SearchOptions searchOptions, BackgroundWorker worker)
        {
            if (DocumentHierarchy == null)
            {
                return null;
            }
            var node = path == null ? DocumentHierarchy.RootNode : DocumentHierarchy.GetNode(path);
            if (node == null || String.IsNullOrWhiteSpace(searchOptions.SearchText))
            {
                return node;
            }
            IList<INode> filteredNodes = DocumentHierarchy
				.SearchNodesFullText(searchOptions.SearchText, searchOptions.SearchField)
                .Where(result => result.Path.StartsWith(node.Path)) // TODO: filter it earlier for performance
                .Select(result => result.Node)
                .ToList();
			ReportProgress(worker);
            return FillNewFilteredDocumentHierarchyRecursively(filteredNodes, node, null, worker);
        }

        public INode GetFunctionNodeByName(string name)
        {
            if (DocumentHierarchy == null)
            {
                return null;
            }
            return DocumentHierarchy
				.SearchNodesByTerm(name, FullTextFieldType.NameNotAnalyzed)
                .Select(result => result.Node)
                .FirstOrDefault(node => node.NodeType == NodeType.Function);
        }

        private INode CreateNewViewNodeWithParents(INode node)
        {
            var parent = node.Parent == null ? null : CreateNewViewNodeWithParents(node.Parent);
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
                viewNode = CreateNewViewNodeWithParents(node);
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
				FillNewFilteredDocumentHierarchyRecursively(filteredNodes, child, viewNode, worker);
            }
			ReportProgress(worker);
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
