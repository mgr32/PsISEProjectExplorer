using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.UI.ViewModel
{
    public class DefaultTreeViewEntryItemComparer : IComparer<TreeViewEntryItemModel>
    {
        public static IComparer<TreeViewEntryItemModel> TREEVIEWENTRYITEM_COMPARER = new DefaultTreeViewEntryItemComparer();

        public int Compare(TreeViewEntryItemModel x, TreeViewEntryItemModel y)
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
            return DefaultNodeComparer.NODE_COMPARER.Compare(x.Node, y.Node);
        }
    }
}
