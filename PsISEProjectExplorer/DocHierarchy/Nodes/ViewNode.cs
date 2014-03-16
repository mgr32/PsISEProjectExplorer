using PsISEProjectExplorer.EnumsAndOptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.DocHierarchy.Nodes
{
    public class ViewNode : INode
    {
        public NodeType NodeType { get { return NodeType.INTERMEDIATE; } }

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

        public int OrderValue
        {
            get
            {
                return this.Node.OrderValue;
            }
        }

        public INode Parent { get; private set; }

        public ICollection<INode> Children { get; private set; }

        public ViewNode(INode viewedNode, INode parent)
        {
            if (viewedNode == null)
            {
                throw new ArgumentNullException("viewedNode");
            }
            this.Node = viewedNode;
            this.Parent = parent;
            this.Children = new SortedSet<INode>(DefaultNodeComparer.NODE_COMPARER);
            if (this.Parent != null)
            {
                this.Parent.Children.Add(this);
            }
        }
    }
}
