using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.Model;
using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using PsISEProjectExplorer.Services;
using PsISEProjectExplorer.UI.Helpers;
using PsISEProjectExplorer.UI.IseIntegration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PsISEProjectExplorer.UI.ViewModel
{
    [Component]
    public class TreeViewModel : BaseViewModel
    {
        public IEnumerable<TreeViewEntryItemModel> TreeViewItems
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

        private TreeViewEntryItemModel rootTreeViewEntryItem;

        public TreeViewEntryItemModel RootTreeViewEntryItem
        {
            get
            {
                return this.rootTreeViewEntryItem;
            }
            private set
            {
                this.rootTreeViewEntryItem = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged("TreeViewItems");
            }
        }

        private int numberOfFiles;

        public int NumberOfFiles
        {
            get
            {
                return this.numberOfFiles;
            }
            private set
            {
                this.numberOfFiles = value;
                this.OnPropertyChanged();
            }
        }

        public TreeViewEntryItemModel SelectedItem
        {
            get
            {
                return (TreeViewEntryItemModel) this.ProjectExplorerWindow.SearchResultsTreeView.SelectedItem;
            }
        }

        public string PathOfItemToSelectOnRefresh { get; set; }

        private IDictionary<string, TreeViewEntryItemModel> ItemsMap { get; set; }

        private IconProvider IconProvider { get; set; }

        // TODO: this is just to get SelectedItem -> should be achievable by binding
        private ProjectExplorerWindow ProjectExplorerWindow { get; set; }
             
        public TreeViewModel(IconProvider iconProvider, ProjectExplorerWindow projectExplorerWindow)
        {
            this.ItemsMap = new Dictionary<string, TreeViewEntryItemModel>();
            this.IconProvider = iconProvider;
            this.ProjectExplorerWindow = projectExplorerWindow;
        }

        public void ReRoot(INode rootNode)
        {
            lock (TreeViewEntryItemModel.RootLockObject)
            {
                this.ItemsMap.Clear();
                this.RootTreeViewEntryItem = rootNode == null ? null : this.CreateTreeViewEntryItemModel(rootNode, null, false);
                this.NumberOfFiles = 0;
            }
        }

        public TreeViewEntryItemModel FindTreeViewEntryItemByPath(string path)
        {
            lock (TreeViewEntryItemModel.RootLockObject)
            {
                TreeViewEntryItemModel result;
                this.ItemsMap.TryGetValue(path, out result);
                return result;
            }
        }

        // running in Indexing or UI thread
        public void RefreshFromNode(INode node, string path, bool expandAllNodes)
        {
            // node == null -> search returned no results at all
            if (node == null)
            {
                this.HandleNoResultsFound(path);
                return;
            }
            TreeViewEntryItemModel treeViewEntryItem = this.FindTreeViewEntryItemByPath(node.Path);
            if (treeViewEntryItem == null)
            {
                bool isSelected = node.Path == this.PathOfItemToSelectOnRefresh;
                treeViewEntryItem = this.CreateTreeViewEntryItemModelWithNodeParents(node, isSelected, expandAllNodes);
            }
            else
            {
                this.UpdateNode(treeViewEntryItem, node);
                // also refresh parents (icons can change)
                var parent = treeViewEntryItem.Parent;
                while (parent != null)
                {
                    parent.RefreshNode();
                    parent = parent.Parent;
                }
            }
            if (expandAllNodes)
            {
                treeViewEntryItem.IsExpanded = true;
            }
            this.RefreshFromIntermediateNode(node, treeViewEntryItem, expandAllNodes);
            this.OnPropertyChanged("TreeViewItems");
        }

        private void UpdateNode(TreeViewEntryItemModel treeViewEntryItem, INode node)
        {
            treeViewEntryItem.UpdateNode(node);
        }

        private void HandleNoResultsFound(string path)
        {
            // path == null -> search was run on whole tree and gave no results -> just clear the tree
            if (path == null)
            {
                this.ReRoot(this.RootTreeViewEntryItem.Node);
                return;
            }

            // path != null -> search was run on a subtree and gave no results -> remove the item
            var item = this.FindTreeViewEntryItemByPath(path);
            if (item != null)
            {
                this.DeleteTreeViewEntryItemModel(item);
            }
        }

        // TODO: this seems to suffer from race conditions
        private void RefreshFromIntermediateNode(INode node, TreeViewEntryItemModel treeViewEntryItem, bool expandAllNodes)
        {
            if (node == null || treeViewEntryItem == null)
            {
                return;
            }
            IList<INode> nodeChildren;
            lock (node)
            {
                nodeChildren = new List<INode>(node.Children);
            }
            // delete old items
            IList<TreeViewEntryItemModel> itemsToDelete = treeViewEntryItem.Children.Where(item => !nodeChildren.Contains(item.Node)).ToList();
            foreach (TreeViewEntryItemModel item in itemsToDelete) {
                this.DeleteTreeViewEntryItemModel(item);
            }

            // add new items
            foreach (INode docHierarchyChild in nodeChildren)
            {
                TreeViewEntryItemModel newTreeViewItem;
                lock (treeViewEntryItem)
                {
                     newTreeViewItem = treeViewEntryItem.Children
                        .FirstOrDefault(treeViewChild => docHierarchyChild.Equals(treeViewChild.Node));
                    if (newTreeViewItem == null)
                    {
                        bool isSelected = docHierarchyChild.Path == this.PathOfItemToSelectOnRefresh;
                        newTreeViewItem = this.CreateTreeViewEntryItemModel(docHierarchyChild, treeViewEntryItem, isSelected);
                    }
                    else
                    {
                        this.UpdateNode(newTreeViewItem, docHierarchyChild);
                    }
                    if (expandAllNodes)
                    {
                        newTreeViewItem.IsExpanded = true;
                    }
                }
                this.RefreshFromIntermediateNode(docHierarchyChild, newTreeViewItem, expandAllNodes);
            }
        }

        public TreeViewEntryItemModel CreateTreeViewEntryItemModel(INode node, TreeViewEntryItemModel parent, bool isSelected)
        {
            if (node == null)
            {
                return null;
            }
            var lockObject = parent == null ? TreeViewEntryItemModel.RootLockObject : parent;
            lock (lockObject)
            {
                var item = new TreeViewEntryItemModel(node, parent, isSelected, this.IconProvider);
                this.ItemsMap[node.Path] = item;
                if (node.NodeType == NodeType.File)
                {
                    lock (TreeViewEntryItemModel.RootLockObject)
                    {
                        this.NumberOfFiles++;
                    }
                }
                return item;
            }
        }

        private TreeViewEntryItemModel CreateTreeViewEntryItemModelWithNodeParents(INode node, bool isSelected, bool expandAllNodes)
        {
            if (node == null)
            {
                return null;
            }
            var itemParent = node.Parent == null ? null : this.CreateTreeViewEntryItemModelWithNodeParents(node.Parent, false, expandAllNodes);
            TreeViewEntryItemModel item = this.FindTreeViewEntryItemByPath(node.Path);
            if (item == null)
            {
                item = this.CreateTreeViewEntryItemModel(node, itemParent, isSelected);
            }
            if (expandAllNodes)
            {
                item.IsExpanded = true;
            }
            return item;
        }

        public void DeleteTreeViewEntryItemModel(TreeViewEntryItemModel item, bool first = true)
        {
            if (item == this.RootTreeViewEntryItem || item == null)
            {
                return;
            }
            var lockObject = item.Parent == null ? TreeViewEntryItemModel.RootLockObject : item.Parent;
            IList<TreeViewEntryItemModel> children;
            lock (lockObject)
            {
                this.ItemsMap.Remove(item.Path);
                if (item.NodeType == NodeType.File)
                {
                    lock (TreeViewEntryItemModel.RootLockObject)
                    {
                        this.NumberOfFiles--;
                    }
                }
                children = new List<TreeViewEntryItemModel>(item.Children);
            }
            foreach (var child in children)
            {
                this.DeleteTreeViewEntryItemModel(child, false);
            }
            if (first)
            {
                item.Delete();
            }
        }
    }
}
