using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.Model.DocHierarchy.Nodes
{
    public class DefaultNodeComparer : IComparer<INode>
    {
        public static IComparer<INode> NODE_COMPARER = new DefaultNodeComparer();

        public int Compare(INode x, INode y)
        {
            if (x == null && y == null)
            {
                return 0;
            }
            if (x == null && y != null)
            {
                return -1;
            }
            if (x != null && y == null)
            {
                return 1;
            }
            if (x.OrderValue != y.OrderValue)
            {
                return x.OrderValue.CompareTo(y.OrderValue);
            }
            return x.Name.CompareTo(y.Name);
        }
    }
}
