using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using System.Collections.Generic;

namespace PsISEProjectExplorer.UI.ViewModel
{
    public class DefaultTreeViewEntryItemComparer : IComparer<TreeViewEntryItemModel>
    {
        public static readonly IComparer<TreeViewEntryItemModel> TreeViewEntryItemComparer = new DefaultTreeViewEntryItemComparer();

        public int Compare(TreeViewEntryItemModel x, TreeViewEntryItemModel y)
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
            return DefaultNodeComparer.NodeComparer.Compare(x.Node, y.Node);
        }
    }
}
