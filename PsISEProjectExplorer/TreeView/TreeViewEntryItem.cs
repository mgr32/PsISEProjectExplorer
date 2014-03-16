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
                INode node = this.Node;
                if (node is DirectoryNode)
                {
                    return "Resources/folder.png";
                }
                else if (node is FileNode)
                {
                    return "Resources/page_white.png";
                }
                return null;
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

        public ObservableCollection<TreeViewEntryItem> Children { get; set; }

        public TreeViewEntryItem(INode node)
        {
            this.DocumentHierarchyNode = node;
            this.Children = new ObservableCollection<TreeViewEntryItem>();
        }
    }
}
