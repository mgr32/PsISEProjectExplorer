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

        private string name;

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
                this.OnPropertyChanged();
            }
        }

        public string Path { get; private set; }
        
        private TreeViewEntryItemModel Parent { get; set; }

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

        public NodeType NodeType { get; private set; }

        public TreeViewEntryItemObservableSet Children { get; private set; }

        public TreeViewEntryItemModel(TreeViewEntryItemModel parent, string nodePath, NodeType nodeType)
        {
            this.Parent = parent;
            this.Children = new TreeViewEntryItemObservableSet();
            this.Name = string.Empty;
            this.Path = nodePath;
            if (this.Parent != null)
            {
                this.Parent.Children.Add(this);
            }
            this.IsSelected = true;
            this.NodeType = nodeType;
        }

        public TreeViewEntryItemModel(INode node, TreeViewEntryItemModel parent, bool isSelected)
        {
            if (node == null) 
            {
                throw new ArgumentNullException("node");
            }
            this.DocumentHierarchyNode = node;
            this.Path = node.Path;
            this.Parent = parent;
            this.Children = new TreeViewEntryItemObservableSet();
            this.Name = this.DocumentHierarchyNode.Name;
            this.NodeType = node.NodeType;
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

        public void UpdateNewItem(string newPath, string newName)
        {
            if (this.Node != null)
            {
                throw new InvalidOperationException("Path/name can be updated only for new items");
            }
            this.Path = newPath;
            this.Name = newName;
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
