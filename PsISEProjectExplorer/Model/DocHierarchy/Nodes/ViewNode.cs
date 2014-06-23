using PsISEProjectExplorer.Enums;
using System;
using System.Collections.Generic;

namespace PsISEProjectExplorer.Model.DocHierarchy.Nodes
{
    public class ViewNode : INode
    {
        public NodeType NodeType { get { return this.Node.NodeType; } }

        public INode Node { get; private set; }

        public string Path
        {
            get
            {
                return this.Node.Path;
            }
        }

        public string Name
        {
            get
            {
                return this.Node.Name;
            }
        }

        public bool IsValid
        {
            get
            {
                return this.Node.IsValid;
            }
        }

        public string Metadata
        {
            get
            {
                return this.Node.Metadata;
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
            this.Node = viewedNode;
            this.Parent = parent;
            this.Children = new SortedSet<INode>(DefaultNodeComparer.NodeComparer);
            if (this.Parent != null)
            {
                this.Parent.Children.Add(this);
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is INode))
            {
                return false;
            }
            var node = (INode)obj;
            return (node.Path == this.Path);
        }

        public override int GetHashCode()
        {
            return this.Path.GetHashCode();
        }

        public void Remove()
        {
            if (this.Parent != null)
            {
                this.Parent.Children.Remove(this);
            }
        }
    }
}
