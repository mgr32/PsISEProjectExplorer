using ProjectExplorer.DocHierarchy;
using ProjectExplorer.DocHierarchy.Nodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ProjectExplorer.TreeView
{
    public class TreeViewEntryItem : TreeViewItem
    {
        public INode DocumentHierarchyNode { get; private set; }

        public TreeViewEntryItem(INode node)
        {
            this.DocumentHierarchyNode = node;
            this.Header = this.DocumentHierarchyNode.Name;
        }
    }
}
