using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.Model;
using PsISEProjectExplorer.Model.DocHierarchy;
using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.Services
{
    public class DocumentHierarchySearcher
    {
        private DocumentHierarchy DocumentHierarchy { get; set; }

        public DocumentHierarchySearcher(DocumentHierarchy documentHierarchy)
        {
            this.DocumentHierarchy = documentHierarchy;
        }

        public INode GetFilteredDocumentHierarchyNodes(string filter, SearchOptions searchOptions)
        {
            lock (this.DocumentHierarchy.RootNode)
            {
                if (String.IsNullOrWhiteSpace(filter))
                {
                    return this.DocumentHierarchy.RootNode;
                }

                INode newRoot = new RootNode(this.DocumentHierarchy.RootNode.Path);
                IEnumerable<INode> nodes = this.DocumentHierarchy
                                                .SearchNodesFullText(filter, searchOptions.SearchField)
                                                .Select(result => result.Node);
                if (searchOptions.IncludeAllParents)
                {
                    this.FillNewFilteredDocumentHierarchyRecursively(nodes, newRoot, this.DocumentHierarchy.RootNode);
                }
                else
                {
                    this.FillNewDocumentHierarchyRecursively(nodes, newRoot);
                }

                return newRoot;
            }
        }

        public INode GetFunctionNodeByName(string name)
        {
            lock (this.DocumentHierarchy.RootNode)
            {
                return this.DocumentHierarchy
                            .SearchNodesByTerm(name, FullTextFieldType.NAME_NOT_ANALYZED)
                            .Select(result => result.Node)
                            .Where(node => node.NodeType == NodeType.FUNCTION)
                            .FirstOrDefault();
            }
        }

        private void FillNewDocumentHierarchyRecursively(IEnumerable<INode> filteredNodes, INode newParent) 
        {
            foreach (INode node in filteredNodes)
            {
                var newNode = new ViewNode(node, newParent);
                if (node.Children.Any())
                {
                    this.FillNewDocumentHierarchyRecursively(node.Children, newNode);
                }
            }
        }

        private void FillNewFilteredDocumentHierarchyRecursively(IEnumerable<INode> filteredNodes, INode newParent, INode oldParent)
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
                        this.FillNewFilteredDocumentHierarchyRecursively(filteredNodes, newNode, node);
                    }
                }
            }
        }
    }
}
