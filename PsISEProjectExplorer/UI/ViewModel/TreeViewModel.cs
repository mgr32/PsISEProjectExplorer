using PsISEProjectExplorer.EnumsAndOptions;
using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using PsISEProjectExplorer.Services;
using PsISEProjectExplorer.UI.IseIntegration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.UI.ViewModel
{
    public class TreeViewModel : BaseViewModel
    {
        public TreeViewEntryItemModel RootTreeViewEntryItem { get; private set; }

        public TreeViewEntryItemObservableSet TreeViewItems
        {
            get
            {
                if (this.RootTreeViewEntryItem == null)
                {
                    return new TreeViewEntryItemObservableSet();
                }
                return this.RootTreeViewEntryItem.Children;
            }
        }

        public TreeViewOptions TreeViewOptions { get; set; }

        public IseIntegrator IseIntegrator { get; set; }

        public TreeViewModel()
        {
            this.TreeViewOptions = new TreeViewOptions();
        }

        public void RefreshFromRoot(INode newDocumentHierarchyRoot)
        {
            if (newDocumentHierarchyRoot == null)
            {
                this.RootTreeViewEntryItem = null;
                FileSystemChangeNotifier.Watch(null);
                return;
            }

            if (this.RootTreeViewEntryItem == null || !this.RootTreeViewEntryItem.Node.Equals(newDocumentHierarchyRoot))
            {
                this.RootTreeViewEntryItem = new TreeViewEntryItemModel(newDocumentHierarchyRoot, null);
                FileSystemChangeNotifier.Watch(this.RootTreeViewEntryItem.Node.Path);
            }

            this.RefreshFromIntermediateNode(newDocumentHierarchyRoot, this.RootTreeViewEntryItem);
            this.OnPropertyChanged("TreeViewItems");
        }

        private void RefreshFromIntermediateNode(INode node, TreeViewEntryItemModel treeViewEntryItem)
        {
            
            // delete old items
            var itemsToDelete = treeViewEntryItem.Children.Where(item => !node.Children.Contains(item.Node)).ToList();
            foreach (TreeViewEntryItemModel item in itemsToDelete) {
                item.Delete();
            }

            // add new items
            foreach (INode docHierarchyChild in node.Children)
            {
                TreeViewEntryItemModel newTreeViewItem = null;
                foreach (TreeViewEntryItemModel treeViewChild in treeViewEntryItem.Children)
                {
                    if (treeViewChild.Node.Equals(docHierarchyChild))
                    {
                        newTreeViewItem = treeViewChild;
                        break;
                    }
                }
                if (newTreeViewItem == null)
                {
                    newTreeViewItem = new TreeViewEntryItemModel(docHierarchyChild, treeViewEntryItem);
                    newTreeViewItem.IsExpanded = this.TreeViewOptions.ExpandAllNodes;
                }
                this.RefreshFromIntermediateNode(docHierarchyChild, newTreeViewItem);
            }

        }

        public void SelectItem(TreeViewEntryItemModel item)
        {
            if (this.IseIntegrator == null)
            {
                throw new InvalidOperationException("IseIntegrator has not ben set yet.");
            }
            if (item != null)
            {
                if (item.Node.NodeType == NodeType.FILE)
                {
                    this.IseIntegrator.GoToFile(item.Node.Path);
                }
                else if (item.Node.NodeType == NodeType.FUNCTION)
                {
                    PowershellFunctionNode node = ((PowershellFunctionNode)item.Node);
                    this.IseIntegrator.GoToFile(node.FilePath);
                    this.IseIntegrator.SetCursor(node.PowershellFunction.StartLine, node.PowershellFunction.StartColumn);
                }
            }
        }

    }
}
