using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using PsISEProjectExplorer.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PsISEProjectExplorer.UI.ViewModel
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

        public static void MapToTreeViewEntryItem(INode node, TreeViewEntryItem treeViewEntryItem, bool expandNodes)
        {
            foreach (INode child in node.Children)
            {
                TreeViewEntryItem newItem = new TreeViewEntryItem(child);
                newItem.IsExpanded = expandNodes;
                treeViewEntryItem.Children.Add(newItem);
                MapToTreeViewEntryItem(child, newItem, expandNodes);
            }
        }
    }
}
