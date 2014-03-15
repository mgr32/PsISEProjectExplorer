using ProjectExplorer.DocHierarchy;
using ProjectExplorer.DocHierarchy.Nodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectExplorer.TreeView
{
    public class DocumentHierarchySearcher
    {

        private DocumentHierarchy documentHierarchy;

        public DocumentHierarchySearcher(DocumentHierarchy documentHierarchy)
        {
            this.documentHierarchy = documentHierarchy;
        }

        public INode GetFilteredDocumentHierarchyNodes(string filter, SearchOptions searchOptions)
        {
            
            if (String.IsNullOrWhiteSpace(filter))
            {
                return this.documentHierarchy.RootNode;
            }
            
            INode newRoot = new RootNode(this.documentHierarchy.RootNode.Path);
            IEnumerable<INode> nodes = documentHierarchy.SearchNodesFullText(filter);
            if (searchOptions.IncludeAllParents)
            {
                this.FillNewFilteredDocumentHierarchyRecursively(nodes, newRoot, documentHierarchy.RootNode);
            }
            else
            {
                this.FillNewDocumentHierarchyRecursively(nodes, newRoot);
            }
        
           return newRoot;
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

       /* private void FillFilteredTreeViewItemsRecursively(IEnumerable<DocumentHierarchyNode> filteredNodes, DocumentHierarchyNode newParent, DocumentHierarchyNode originalParent)
        {
            if (!newParent.Children.Any())
            {
                return;
            }
            var sortedChildrenNodes = newParent.Children.OrderBy(node => node is DocumentHierarchyDirectoryNode).ThenBy(node => node.Name);
            foreach (DocumentHierarchyNode node in sortedChildrenNodes)
            {
                if (filteredNodes.Any(filteredNode => filteredNode.Path.StartsWith(node.Path)))
                {
                    var newDocumentHierarchyNode = new DocumentHierarchyNode(node.Path, node.Name, newParent);
                    if (node.Children.Any())
                    {
                        this.FillFilteredTreeViewItemsRecursively(filteredNodes, newDocumentHierarchyNode);
                    }
                }
            }
        }*/

    }
}
