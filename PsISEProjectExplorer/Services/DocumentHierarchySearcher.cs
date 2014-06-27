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

        public DocumentHierarchySearcher(DocumentHierarchy documentHierarchy)
        {
            this.DocumentHierarchy = documentHierarchy;
        }

        public INode GetFilteredDocumentHierarchyNodes(string filter, SearchOptions searchOptions, BackgroundWorker worker)
        {
            if (this.DocumentHierarchy == null)
            {
                return null;
            }
            lock (this.DocumentHierarchy.RootNode)
            {
                if (String.IsNullOrWhiteSpace(filter))
                {
                    return this.DocumentHierarchy.RootNode;
                }

                INode newRoot = new RootNode(this.DocumentHierarchy.RootNode.Path);
                ICollection<INode> nodes = this.DocumentHierarchy
                    .SearchNodesFullText(filter, searchOptions.SearchField)
                    .Select(result => result.Node)
                    .ToList();
                this.ReportProgress(worker);
                if (searchOptions.IncludeAllParents)
                {
                    this.FillNewFilteredDocumentHierarchyRecursively(nodes, newRoot, this.DocumentHierarchy.RootNode, worker);
                }
                else
                {
                    this.FillNewDocumentHierarchyRecursively(nodes, newRoot, worker);
                }

                return newRoot;
            }
        }

        public INode GetFunctionNodeByName(string name)
        {
            if (this.DocumentHierarchy == null)
            {
                return null;
            }
            lock (this.DocumentHierarchy.RootNode)
            {
                return this.DocumentHierarchy
                    .SearchNodesByTerm(name, FullTextFieldType.NameNotAnalyzed)
                    .Select(result => result.Node)
                    .FirstOrDefault(node => node.NodeType == NodeType.Function);
            }
        }

        private void FillNewDocumentHierarchyRecursively(IEnumerable<INode> filteredNodes, INode newParent, BackgroundWorker worker) 
        {
            foreach (INode node in filteredNodes)
            {
                var newNode = new ViewNode(node, newParent);
                if (node.Children.Any())
                {
                    this.FillNewDocumentHierarchyRecursively(node.Children, newNode, worker);
                }
            }
            this.ReportProgress(worker);
        }

        private void FillNewFilteredDocumentHierarchyRecursively(ICollection<INode> filteredNodes, INode newParent, INode oldParent, BackgroundWorker worker)
        {
            if (!oldParent.Children.Any())
            {
                return;
            }
            foreach (INode node in oldParent.Children)
            {
                if (filteredNodes.Any(filteredNode => filteredNode.Path.StartsWith(node.Path)))
                {
                    var newNode = new ViewNode(node, newParent);
                    if (node.Children.Any())
                    {
                        this.FillNewFilteredDocumentHierarchyRecursively(filteredNodes, newNode, node, worker);
                    }
                }
            }
            this.ReportProgress(worker);
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
