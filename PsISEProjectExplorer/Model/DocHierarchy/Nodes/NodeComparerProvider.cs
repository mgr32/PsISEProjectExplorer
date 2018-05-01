using System.Collections.Generic;

namespace PsISEProjectExplorer.Model.DocHierarchy.Nodes
{
    public class NodeComparerProvider
    {
        public static bool SortFunctions { get; set; }

        public static IComparer<INode> NodeComparer
        {
            get
            {
                return SortFunctions ? NodeComparerAlphabetical.NodeComparer : NodeComparerAsIs.NodeComparer;
            }
        }
    }
}
