using ProjectExplorer.DocHierarchy;
using ProjectExplorer.DocHierarchy.Nodes;
using ProjectExplorer.EnumsAndOptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectExplorer.DocHierarchy.HierarchyLogic
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
            IEnumerable<INode> nodes = documentHierarchy.SearchNodesFullText(filter, searchOptions);
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
    }
}
