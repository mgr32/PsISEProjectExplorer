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
    [Component]
    public class DocumentHierarchySearcher
    {
        
        // note: can be invoked by multiple threads simultaneously
        public INode GetDocumentHierarchyViewNodeProjection(DocumentHierarchy documentHierarchy, string path, SearchOptions searchOptions, BackgroundWorker worker)
        {
            if (documentHierarchy == null)
            {
                return null;
            }
            var node = path == null ? documentHierarchy.RootNode : documentHierarchy.GetNode(path);
            if (node == null || String.IsNullOrWhiteSpace(searchOptions.SearchText))
            {
                return node;
            }
            IList<INode> filteredNodes = documentHierarchy
                .SearchNodesFullText(searchOptions)
                .Where(result => result.Path.StartsWith(node.Path)) // TODO: filter it earlier for performance
                .Select(result => result.Node)
                .ToList();
            this.ReportProgress(worker);
            return this.FillNewFilteredDocumentHierarchyRecursively(filteredNodes, node, null, worker);
        }

        public INode GetFunctionNodeByName(DocumentHierarchy documentHierarchy, string name, string currentFilePath)
        {
            if (documentHierarchy == null)
            {
                return null;
            }
            return documentHierarchy
                .SearchNodesByTerm(name, FullTextFieldType.NameNotAnalyzed)
                .Select(result => result.Node)
                .Where(node => node.NodeType != NodeType.Directory && node.NodeType != NodeType.File && node.NodeType != NodeType.Intermediate)
                .OrderBy(node => hasParentWithPath(node, currentFilePath) ? 0 : 1)
                .FirstOrDefault();
        }

        private bool hasParentWithPath(INode node, string path)
        {
            if (node == null)
            {
                return false;
            }
            if (node.Path == path)
            {
                return true;
            }
            return hasParentWithPath(node.Parent, path);
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
