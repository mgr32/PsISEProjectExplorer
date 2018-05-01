using System;
using System.Collections.Generic;

namespace PsISEProjectExplorer.Model.DocHierarchy.Nodes
{
    public class NodeComparerAsIs : IComparer<INode>
    {
        public static readonly IComparer<INode> NodeComparer = new NodeComparerAsIs();

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
            bool bothItemsArePowershellItemNode = x is PowershellItemNode && y is PowershellItemNode;
            if (x.NodeType != y.NodeType && !bothItemsArePowershellItemNode)
            {
                return x.NodeType.CompareTo(y.NodeType);
            }
            return bothItemsArePowershellItemNode ? CompareLines(x, y) : CompareNames(x, y);
        }

        private int CompareNames(INode x, INode y)
        {
            int nameCompare = string.Compare(x.Name, y.Name, StringComparison.InvariantCulture);
            if (nameCompare != 0)
            {
                return nameCompare;
            }
            return string.Compare(x.Path, y.Path, StringComparison.InvariantCulture);
        }

        private int CompareLines(INode x, INode y)
        {
            PowershellItemNode psNode1 = x as PowershellItemNode;
            PowershellItemNode psNode2 = y as PowershellItemNode;
            if (psNode1.PowershellItem == null && psNode2.PowershellItem == null)
            {
                return 0;
            }
            if (psNode1.PowershellItem == null)
            {
                return -1;
            }
            if (psNode2.PowershellItem == null)
            {
                return 1;
            }
            int lineNumberCompare = psNode1.PowershellItem.StartLine - psNode2.PowershellItem.StartLine;
            if (lineNumberCompare != 0)
            {
                return lineNumberCompare;
            }
            return psNode1.PowershellItem.StartColumn - psNode2.PowershellItem.StartColumn;
        }
    }
}
