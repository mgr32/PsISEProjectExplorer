using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using System;

namespace PsISEProjectExplorer.UI.ViewModel
{
	public class TreeViewEntryItemModel : BaseViewModel
    {
        public static object RootLockObject = new object();

        private INode documentHierarchyNode;

        private INode DocumentHierarchyNode { 
            get
            {
                return documentHierarchyNode;
            }
            set
            {
				documentHierarchyNode = value;
				OnPropertyChanged("IsExpanded");
            }
        }

        public INode Node
        {
            get
            {
                var node = DocumentHierarchyNode as ViewNode;
                if (node != null)
                {
                    return node.Node;
                }
                return DocumentHierarchyNode;
            }
        }

        public string Image
        {
            get
            {
                string fileName = NodeType.ToString().ToLowerInvariant();
                if (!Node.IsValid)
                {
                    fileName += "_invalid";
                }
                return String.Format("Resources/{0}.png", fileName);
            }
        }

        public string Name
        {
            get
            {
                return Node.Name;
            }
        }

        public string Path
        {
            get
            {
                return Node.Path;
            }
        }

        public string Tooltip
        {
            get
            {
                return Node.Metadata;
            }
        }

        public TreeViewEntryItemModel Parent { get; private set; }

        private TreeViewEntryItemModelState State { get; set; }

        public bool IsExpanded
        {
            get
            {
                return State.IsExpanded;
            }
            set
            {
				State.IsExpanded = value;
				OnPropertyChanged();
            }
        }

        public bool IsSelected
        {
            get
            {
                return State.IsSelected;
            }
            set
            {
				State.IsSelected = value;
				OnPropertyChanged();
            }
        }
        

        private bool isBeingEdited;

        public bool IsBeingEdited
        {
            get
            {
                return isBeingEdited;
            }
            set
            {
				isBeingEdited = value;
				OnPropertyChanged();
            }
        }

        private bool isBeingAdded;

        public bool IsBeingAdded
        {
            get
            {
                return isBeingAdded;
            }
            set
            {
				isBeingAdded = value;
				OnPropertyChanged();
            }
        }

        public NodeType NodeType
        {
            get
            {
                return Node.NodeType;
            }
        }

        public TreeViewEntryItemObservableSet Children { get; private set; }

        public TreeViewEntryItemModel(INode node, TreeViewEntryItemModel parent, bool isSelected)
        {
            if (node == null) 
            {
                throw new ArgumentNullException("node");
            }
            var lockObject = Parent == null ? RootLockObject : Parent;
            lock (lockObject)
            {
				State = new TreeViewEntryItemModelState(false, isSelected);
				DocumentHierarchyNode = node;
				Parent = parent;
				Children = new TreeViewEntryItemObservableSet();
                if (Parent != null)
                {
					Parent.Children.Add(this);
                }
            }
        }

        public void Delete()
        {
            var lockObject = Parent == null ? RootLockObject : Parent;
            lock (lockObject)
            {
                if (Parent != null)
                {
					Parent.Children.Remove(this);
                }
				DocumentHierarchyNode = null;
				Children = null;
				Parent = null;
            }
        }

        public void UpdateNode(INode node)
        {
            if (DocumentHierarchyNode != node)
            {
				DocumentHierarchyNode = node;
				RefreshNode();
            }
        }

        public void RefreshNode()
        {
			OnPropertyChanged(String.Empty);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType() || !(obj is TreeViewEntryItemModel))
            {
                return false;
            }
            var item = (TreeViewEntryItemModel)obj;
            return (Node == item.Node);
        }

        public override int GetHashCode()
        {
            return (Node == null ? 0 : Node.GetHashCode());
        }

    }
}
