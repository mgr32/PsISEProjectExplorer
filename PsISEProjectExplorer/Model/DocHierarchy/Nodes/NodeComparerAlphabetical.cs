using System;
using System.Collections.Generic;

namespace PsISEProjectExplorer.Model.DocHierarchy.Nodes
{
    public class NodeComparerAlphabetical : IComparer<INode>
    {
        public static readonly IComparer<INode> NodeComparer = new NodeComparerAlphabetical();

        public int Compare(INode x, INode y)
        {
            if (x == null && y == null)
            {
                return 0;
            }
            if (x == null)
            {
                return -1;
            }
            if (y == null)
            {
                return 1;
            }
            if (x.NodeType != y.NodeType)
            {
                return x.NodeType.CompareTo(y.NodeType);
            }
            int nameCompare = string.Compare(x.Name, y.Name, StringComparison.InvariantCulture);
            if (nameCompare != 0)
            {
                return nameCompare;
            }
            return string.Compare(x.Path, y.Path, StringComparison.InvariantCulture);
        }
    }
}
