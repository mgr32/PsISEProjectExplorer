using ProjectExplorer.DocHierarchy;
using ProjectExplorer.DocHierarchy.Nodes;
using ProjectExplorer.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ProjectExplorer.TreeView
{
    public class TreeViewEntryItem
    {
        private INode DocumentHierarchyNode { get; set; }

        public INode Node
        {
            get
            {
                if (this.DocumentHierarchyNode is ViewNode)
                {
                    return ((ViewNode)this.DocumentHierarchyNode).Node;
                }
                else
                {
                    return this.DocumentHierarchyNode;
                }                   
            }
        }

        public string Image
        {
            get
            {
                return "Resources/" + this.Node.NodeType.ToString().ToLowerInvariant() + ".png";
            }
        }

        public string Name
        {
            get
            {
                return this.DocumentHierarchyNode.Name;
            }
        }

        public bool IsExpanded { get; set; }

        public IList<TreeViewEntryItem> Children { get; set; }

        public TreeViewEntryItem(INode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            this.DocumentHierarchyNode = node;
            this.Children = new List<TreeViewEntryItem>();
        }
    }
}
