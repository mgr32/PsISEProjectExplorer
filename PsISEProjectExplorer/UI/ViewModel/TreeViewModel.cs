using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.Model;
using PsISEProjectExplorer.Model.DocHierarchy;
using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using PsISEProjectExplorer.Services;
using PsISEProjectExplorer.UI.Helpers;
using PsISEProjectExplorer.UI.IseIntegration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PsISEProjectExplorer.UI.ViewModel
{
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

        public IseIntegrator IseIntegrator { private get; set; }

        private TreeViewEntryItemModel RootTreeViewEntryItem { get; set; }

        public string PathOfItemToSelectOnRefresh { get; set; }

        public void RefreshFromRoot(INode newDocumentHierarchyRoot, bool expandAllNodes, FilesPatternProvider filesPatternProvider)
        {
            if (newDocumentHierarchyRoot == null)
            {
                this.SetNewRootItem(null);
                FileSystemChangeNotifier.Watch(null, filesPatternProvider);
                return;
            }

            if (this.RootTreeViewEntryItem == null || !this.RootTreeViewEntryItem.Node.Equals(newDocumentHierarchyRoot))
            {
                var newRootItem = new TreeViewEntryItemModel(newDocumentHierarchyRoot, null, false);
                this.SetNewRootItem(newRootItem);
                FileSystemChangeNotifier.Watch(newRootItem.Node.Path, filesPatternProvider);
            }
            lock (newDocumentHierarchyRoot)
            {
                this.RefreshFromIntermediateNode(newDocumentHierarchyRoot, this.RootTreeViewEntryItem, expandAllNodes);
            }
            this.OnPropertyChanged("TreeViewItems");
        }

        public TreeViewEntryItemModel FindTreeViewEntryItemByPath(string path)
        {
            return this.FindTreeViewEntryItemByPath(this.RootTreeViewEntryItem, path);
        }

        private TreeViewEntryItemModel FindTreeViewEntryItemByPath(TreeViewEntryItemModel item, string path)
        {
            foreach (TreeViewEntryItemModel child in item.Children)
            {
                if (child.Path == path)
                {
                    return child;
                }
                var result = this.FindTreeViewEntryItemByPath(child, path);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        private void SetNewRootItem(TreeViewEntryItemModel rootItem)
        {
            this.RootTreeViewEntryItem = rootItem;
        }

        private void RefreshFromIntermediateNode(INode node, TreeViewEntryItemModel treeViewEntryItem, bool expandAllNodes)
        {
            
            // delete old items
            var itemsToDelete = treeViewEntryItem.Children.Where(item => !node.Children.Contains(item.Node)).ToList();
            foreach (TreeViewEntryItemModel item in itemsToDelete) {
                item.Delete();
            }

            // add new items
            IList<INode> childrenToIterate = new List<INode>(node.Children);
            foreach (INode docHierarchyChild in childrenToIterate)
            {
                TreeViewEntryItemModel newTreeViewItem = 
                    treeViewEntryItem.Children
                    .FirstOrDefault(treeViewChild => treeViewChild.Node.Equals(docHierarchyChild));
                if (newTreeViewItem == null)
                {
                    bool isSelected = docHierarchyChild.Path == PathOfItemToSelectOnRefresh;
                    newTreeViewItem = new TreeViewEntryItemModel(docHierarchyChild, treeViewEntryItem, isSelected);
                }
                else
                {
                    newTreeViewItem.UpdateNode(docHierarchyChild);
                }
                if (expandAllNodes)
                {
                    newTreeViewItem.IsExpanded = true;
                }
                this.RefreshFromIntermediateNode(docHierarchyChild, newTreeViewItem, expandAllNodes);
            }
        }

        public void OpenItem(TreeViewEntryItemModel item, string searchText)
        {
            if (this.IseIntegrator == null)
            {
                throw new InvalidOperationException("IseIntegrator has not ben set yet.");
            }
            if (item == null)
            {
                return;
            }

            if (item.Node.NodeType == NodeType.File)
            {
                bool wasOpen = (this.IseIntegrator.SelectedFilePath == item.Node.Path);
                if (!wasOpen)
                {
                    this.IseIntegrator.GoToFile(item.Node.Path);
                }
                else
                {
                    this.IseIntegrator.SetFocusOnCurrentTab();
                }
                if (searchText != null && searchText.Length > 2)
                {
                    EditorInfo editorInfo = (wasOpen ? this.IseIntegrator.GetCurrentLineWithColumnIndex() : null);
                    TokenPosition tokenPos = TokenLocator.LocateNextToken(item.Node.Path, searchText, editorInfo);
                    if (tokenPos.MatchLength > 2)
                    {
                        this.IseIntegrator.SelectText(tokenPos.Line, tokenPos.Column, tokenPos.MatchLength);
                    }
                    else if (string.IsNullOrEmpty(this.IseIntegrator.SelectedText))
                    {
                        tokenPos = TokenLocator.LocateSubtoken(item.Node.Path, searchText);
                        if (tokenPos.MatchLength > 2)
                        {
                            this.IseIntegrator.SelectText(tokenPos.Line, tokenPos.Column, tokenPos.MatchLength);
                        }
                    }
                }
            }
            else if (item.Node.NodeType == NodeType.Function)
            {
                var node = ((PowershellFunctionNode)item.Node);
                this.IseIntegrator.GoToFile(node.FilePath);
                this.IseIntegrator.SelectText(node.PowershellFunction.StartLine, node.PowershellFunction.StartColumn, node.Name.Length);
            }
            else if (item.Node.NodeType == NodeType.Directory)
            {
                item.IsExpanded = !item.IsExpanded;
            }
        }
    }
}
