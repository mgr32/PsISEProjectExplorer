using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.Model;
using PsISEProjectExplorer.Model.DocHierarchy;
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

        private string PathOfItemToSelectOnRefresh { get; set; }

        private FileSystemChangeWatcher FileSystemChangeWatcher { get; set; }

        private DocumentHierarchyFactory DocumentHierarchyFactory { get; set; }

        private FilesPatternProvider FilesPatternProvider { get; set; }

        public TreeViewModel(FileSystemChangeWatcher fileSystemChangeWatcher, DocumentHierarchyFactory documentHierarchyFactory, FilesPatternProvider filesPatternProvider)
        {
            this.FileSystemChangeWatcher = fileSystemChangeWatcher;
            this.DocumentHierarchyFactory = documentHierarchyFactory;
            this.FilesPatternProvider = filesPatternProvider;
        }

        public void Clear()
        {
            this.RootTreeViewEntryItem = null;
            this.FileSystemChangeWatcher.StopWatching();
        }

        public void RefreshFromRoot(INode newDocumentHierarchyRoot, bool expandAllNodes, FilesPatternProvider filesPatternProvider)
        {
            if (newDocumentHierarchyRoot == null)
            {
                this.Clear();
                return;
            }

            if (this.RootTreeViewEntryItem == null || !this.RootTreeViewEntryItem.Node.Equals(newDocumentHierarchyRoot))
            {
                var newRootItem = new TreeViewEntryItemModel(newDocumentHierarchyRoot, null, false);
                this.RootTreeViewEntryItem = newRootItem;
                this.FileSystemChangeWatcher.Watch(newRootItem.Node.Path, filesPatternProvider);
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

        public void DeleteTreeItem(TreeViewEntryItemModel selectedItem)
        {
            if (selectedItem == null)
            {
                return;
            }
            int numFilesInside = 0;
            try
            {
                numFilesInside = Directory.GetFileSystemEntries(selectedItem.Path).Count();
            }
            catch (Exception)
            {
                // ignore - this only has impact on message
            }
            string message = numFilesInside == 0 ?
                String.Format("'{0}' will be deleted permanently.", selectedItem.Path) :
                String.Format("'{0}' will be deleted permanently (together with {1} items inside).", selectedItem.Path, numFilesInside);
            if (MessageBoxHelper.ShowConfirmMessage(message))
            {
                try
                {
                    this.FilesPatternProvider.RemoveAdditionalPath(selectedItem.Path);
                    FileSystemOperationsService.DeleteFileOrDirectory(selectedItem.Path);
                }
                catch (Exception e)
                {
                    MessageBoxHelper.ShowError("Failed to delete: " + e.Message);
                }
            }
        }

        public void AddNewTreeItem(TreeViewEntryItemModel parent, NodeType nodeType)
        {
            if (this.DocumentHierarchyFactory == null)
            {
                return;
            }
            if (parent == null)
            {
                parent = this.RootTreeViewEntryItem;
            }
            parent.IsExpanded = true;
            INode newNode = this.DocumentHierarchyFactory.CreateTemporaryNode(parent.Node, nodeType);
            if (newNode == null)
            {
                return;
            }
            var newItem = new TreeViewEntryItemModel(newNode, parent, true);
            newItem.IsBeingEdited = true;
            newItem.IsBeingAdded = true;
        }

        public void MoveTreeItem(TreeViewEntryItemModel movedItem, TreeViewEntryItemModel destinationItem, string rootDirectory)
        {
            if (movedItem == destinationItem)
            {
                return;
            }
            try
            {
                string newPath;
                // moved to the empty place, i.e. to the workspace directory
                if (destinationItem == null)
                {
                    newPath = this.GenerateNewPathForDir(rootDirectory, movedItem.Name);
                }
                else if (destinationItem.NodeType == NodeType.File)
                {
                    newPath = this.GenerateNewPath(destinationItem.Path, movedItem.Name);
                }
                else if (destinationItem.NodeType == NodeType.Directory)
                {
                    newPath = this.GenerateNewPathForDir(destinationItem.Path, movedItem.Name);
                }
                else
                {
                    return;
                }
                this.FilesPatternProvider.RemoveAdditionalPath(movedItem.Path);
                this.FilesPatternProvider.AddAdditionalPath(newPath);
                FileSystemOperationsService.RenameFileOrDirectory(movedItem.Path, newPath);
                if (destinationItem != null)
                {
                    destinationItem.IsExpanded = true;
                }
            }
            catch (Exception e)
            {
                this.PathOfItemToSelectOnRefresh = null;
                MessageBoxHelper.ShowError("Failed to move: " + e.Message);
            }
        }

        public void EndTreeEdit(string newValue, bool save, TreeViewEntryItemModel selectedItem, bool addFileExtension)
        {
            if (selectedItem == null)
            {
                return;
            }
            selectedItem.IsBeingEdited = false;

            if (selectedItem.NodeType == NodeType.File && addFileExtension && !String.IsNullOrEmpty(newValue) && !this.FilesPatternProvider.DoesFileMatch(newValue))
            {
                newValue += ".ps1";
            }
            if (selectedItem.IsBeingAdded)
            {
                selectedItem.IsBeingAdded = false;
                this.EndAddingTreeItem(newValue, save, selectedItem);
            }
            else
            {
                this.EndRenamingTreeItem(newValue, save, selectedItem);
            }
        }

        private void EndRenamingTreeItem(string newValue, bool save, TreeViewEntryItemModel selectedItem)
        {
            if (!save || String.IsNullOrEmpty(newValue))
            {
                return;
            }

            try
            {
                string oldPath = selectedItem.Path;
                string newPath = this.GenerateNewPath(selectedItem.Path, newValue);
                FileSystemOperationsService.RenameFileOrDirectory(oldPath, newPath);
            }
            catch (Exception e)
            {
                this.PathOfItemToSelectOnRefresh = null;
                MessageBoxHelper.ShowError("Failed to rename: " + e.Message);
            }

        }

        private void EndAddingTreeItem(string newValue, bool save, TreeViewEntryItemModel selectedItem)
        {
            if (!save || String.IsNullOrEmpty(newValue))
            {
                this.DocumentHierarchyFactory.RemoveTemporaryNode(selectedItem.Node);
                selectedItem.Delete();
                return;
            }
            var newPath = this.GenerateNewPath(selectedItem.Path, newValue);
            INode newNode = null;
            if (this.FindTreeViewEntryItemByPath(newPath) != null)
            {
                this.DocumentHierarchyFactory.RemoveTemporaryNode(selectedItem.Node);
                selectedItem.Delete();
                MessageBoxHelper.ShowError("Item '" + newPath + "' already exists.");
                return;
            }
            if (selectedItem.NodeType == NodeType.Directory)
            {
                try
                {
                    newNode = this.DocumentHierarchyFactory.UpdateTemporaryNode(selectedItem.Node, newPath);
                    var parent = selectedItem.Parent;
                    selectedItem.Delete();
                    selectedItem = new TreeViewEntryItemModel(newNode, parent, true);
                    this.FilesPatternProvider.AddAdditionalPath(newPath);
                    FileSystemOperationsService.CreateDirectory(newPath);
                }
                catch (Exception e)
                {
                    if (newNode != null)
                    {
                        newNode.Remove();
                    }
                    if (selectedItem != null)
                    {
                        selectedItem.Delete();
                    }
                    this.PathOfItemToSelectOnRefresh = null;
                    MessageBoxHelper.ShowError("Failed to create directory '" + newPath + "': " + e.Message);
                }
            }
            else if (selectedItem.NodeType == NodeType.File)
            {
                try
                {
                    newNode = this.DocumentHierarchyFactory.UpdateTemporaryNode(selectedItem.Node, newPath);
                    var parent = selectedItem.Parent;
                    selectedItem.Delete();
                    selectedItem = new TreeViewEntryItemModel(newNode, parent, true);
                    this.FilesPatternProvider.AddAdditionalPath(newPath);
                    FileSystemOperationsService.CreateFile(newPath);
                    this.IseIntegrator.GoToFile(newPath);
                }
                catch (Exception e)
                {
                    if (newNode != null)
                    {
                        newNode.Remove();
                    }
                    if (selectedItem != null)
                    {
                        selectedItem.Delete();
                    }
                    this.PathOfItemToSelectOnRefresh = null;
                    MessageBoxHelper.ShowError("Failed to create file '" + newPath + "': " + e.Message);
                }
            }
        }

        private string GenerateNewPath(string currentPath, string newValue)
        {
            var newPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(currentPath), newValue);
            this.PathOfItemToSelectOnRefresh = newPath;
            return newPath;
        }

        private string GenerateNewPathForDir(string currentPath, string newValue)
        {
            var newPath = System.IO.Path.Combine(currentPath, newValue);
            this.PathOfItemToSelectOnRefresh = newPath;
            return newPath;
        }
    }
}
