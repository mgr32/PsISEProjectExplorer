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
    public class TreeViewEntryItemModel : BaseViewModel
    {
        private INode documentHierarchyNode;

        private INode DocumentHierarchyNode { 
            get
            {
                return this.documentHierarchyNode;
            }
            set
            {
                this.documentHierarchyNode = value;
                this.OnPropertyChanged("IsExpanded");
            }
        }

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

        public string Path
        {
            get
            {
                return this.DocumentHierarchyNode.Path;
            }
        }

        public TreeViewEntryItemModel Parent { get; private set; }

        private bool isExpanded;

        public bool IsExpanded
        {
            get
            {
                return this.isExpanded;
            }
            set
            {
                this.isExpanded = value;
                this.OnPropertyChanged();
            }
        }

        public TreeViewEntryItemObservableSet Children { get; private set; }

        public TreeViewEntryItemModel(INode node, TreeViewEntryItemModel parent)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            this.DocumentHierarchyNode = node;
            this.Parent = parent;
            this.Children = new TreeViewEntryItemObservableSet();
            if (this.Parent != null)
            {
                this.Parent.Children.Add(this);
            }
        }

        public void Delete()
        {
            if (this.Parent != null)
            {
                this.Parent.Children.Remove(this);
            }
            this.DocumentHierarchyNode = null;
            this.Children = null;
            this.Parent = null;
        }

        public void UpdateNode(INode node)
        {
            this.DocumentHierarchyNode = node;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || this.GetType() != obj.GetType() || !(obj is TreeViewEntryItemModel))
            {
                return false;
            }
            TreeViewEntryItemModel item = (TreeViewEntryItemModel)obj;
            return (this.Node == item.Node);
        }

        public override int GetHashCode()
        {
            return (this.Node == null ? 0 : this.Node.GetHashCode());
        }

    }
}
