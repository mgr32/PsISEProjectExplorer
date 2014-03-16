using PsISEProjectExplorer.EnumsAndOptions;
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
            
            if (String.IsNullOrWhiteSpace(filter))
            {
                return this.DocumentHierarchy.RootNode;
            }

            INode newRoot = new RootNode(this.DocumentHierarchy.RootNode.Path);
            IEnumerable<INode> nodes = this.DocumentHierarchy.SearchNodesFullText(filter, searchOptions);
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
