using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using PsISEProjectExplorer.Services;
using PsISEProjectExplorer.UI.Helpers;
using System;

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
                var node = this.DocumentHierarchyNode as ViewNode;
                if (node != null)
                {
                    return node.Node;
                }
                return this.DocumentHierarchyNode;
            }
        }

        public string Image
        {
            get
            {
                return "Resources/" + this.NodeType.ToString().ToLowerInvariant() + ".png";
            }
        }

        public string Name
        {
            get
            {
                return this.Node.Name;
            }
        }

        public string Path
        {
            get
            {
                return this.Node.Path;
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

        private bool isSelected;

        public bool IsSelected
        {
            get
            {
                return this.isSelected;
            }
            set
            {
                this.isSelected = value;
                this.OnPropertyChanged();
            }
        }

        private bool isBeingEdited;

        public bool IsBeingEdited
        {
            get
            {
                return this.isBeingEdited;
            }
            set
            {
                this.isBeingEdited = value;
                this.OnPropertyChanged();
            }
        }

        private bool isBeingAdded;

        public bool IsBeingAdded
        {
            get
            {
                return this.isBeingAdded;
            }
            set
            {
                this.isBeingAdded = value;
                this.OnPropertyChanged();
            }
        }

        public NodeType NodeType
        {
            get
            {
                return this.Node.NodeType;
            }
        }

        public TreeViewEntryItemObservableSet Children { get; private set; }

        public TreeViewEntryItemModel(INode node, TreeViewEntryItemModel parent, bool isSelected)
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
            this.IsSelected = isSelected;
            
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
            var item = (TreeViewEntryItemModel)obj;
            return (this.Node == item.Node);
        }

        public override int GetHashCode()
        {
            return (this.Node == null ? 0 : this.Node.GetHashCode());
        }

    }
}
