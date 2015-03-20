using PsISEProjectExplorer.Enums;
using System;
using System.Collections.Generic;

namespace PsISEProjectExplorer.Model.DocHierarchy.Nodes
{
    public class ViewNode : INode
    {
        public NodeType NodeType { get { return Node.NodeType; } }

        public INode Node { get; private set; }

        public string Path
        {
            get
            {
                return Node.Path;
            }
        }

        public string Name
        {
            get
            {
                return Node.Name;
            }
        }

        public bool IsValid
        {
            get
            {
                return Node.IsValid;
            }
        }

        public string Metadata
        {
            get
            {
                return Node.Metadata;
            }
        }


        public INode Parent { get; private set; }

        public ISet<INode> Children { get; private set; }

        public ViewNode(INode viewedNode, INode parent)
        {
            if (viewedNode == null)
            {
                throw new ArgumentNullException("viewedNode");
            }
			Node = viewedNode;
			Parent = parent;
			Children = new SortedSet<INode>(DefaultNodeComparer.NodeComparer);
            if (Parent != null)
            {
				Parent.Children.Add(this);
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is INode))
            {
                return false;
            }
            var node = (INode)obj;
            return (node.Path == Path);
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }

        public void Remove()
        {
            if (Parent != null)
            {
				Parent.Children.Remove(this);
            }
        }
    }
}
