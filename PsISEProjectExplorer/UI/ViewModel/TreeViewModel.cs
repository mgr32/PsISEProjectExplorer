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

        private string PathOfItemToSelectOnRefresh { get; set; }

        private FileSystemChangeWatcher FileSystemChangeWatcher { get; set; }

        private DocumentHierarchyFactory DocumentHierarchyFactory { get; set; }

        private FilesPatternProvider FilesPatternProvider { get; set; }

        private IDictionary<string, TreeViewEntryItemModel> ItemsMap { get; set; }

        public TreeViewModel(FileSystemChangeWatcher fileSystemChangeWatcher, DocumentHierarchyFactory documentHierarchyFactory, FilesPatternProvider filesPatternProvider)
        {
            this.FileSystemChangeWatcher = fileSystemChangeWatcher;
            this.DocumentHierarchyFactory = documentHierarchyFactory;
            this.FilesPatternProvider = filesPatternProvider;
            this.ItemsMap = new Dictionary<string, TreeViewEntryItemModel>();
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

        private void RefreshFromIntermediateNode(INode node, TreeViewEntryItemModel treeViewEntryItem, bool expandAllNodes)
        {
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
                        .FirstOrDefault(treeViewChild => treeViewChild.Node.Equals(docHierarchyChild));
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
                var node = ((PowershellItemNode)item.Node);
                this.IseIntegrator.GoToFile(node.FilePath);
                this.IseIntegrator.SelectText(node.PowershellItem.StartLine, node.PowershellItem.StartColumn, node.Name.Length);
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
            if (!this.HandleUnsavedFileManipulation(selectedItem))
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
                    this.IseIntegrator.CloseFile(selectedItem.Path);
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
            var newItem = this.CreateTreeViewEntryItemModel(newNode, parent, true);
            newItem.IsBeingEdited = true;
            newItem.IsBeingAdded = true;
        }

        public void StartEditingTreeItem(TreeViewEntryItemModel item)
        {
            if (!this.HandleUnsavedFileManipulation(item))
            {
                return;
            }
            item.IsBeingEdited = true;
        }

        public void MoveTreeItem(TreeViewEntryItemModel movedItem, TreeViewEntryItemModel destinationItem, string rootDirectory)
        {
            if (movedItem == destinationItem)
            {
                return;
            }
            if (!this.HandleUnsavedFileManipulation(movedItem))
            {
                return;
            }
            string destPath = destinationItem != null ? destinationItem.Path : rootDirectory;
            if (!MessageBoxHelper.ShowConfirmMessage(String.Format("Please confirm you want to move '{0}' to '{1}'.", movedItem.Path, destPath)))
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
                bool closed = this.IseIntegrator.CloseFile(movedItem.Path);
                FileSystemOperationsService.RenameFileOrDirectory(movedItem.Path, newPath);
                if (closed)
                {
                    this.IseIntegrator.GoToFile(newPath);
                }
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
                bool closed = this.IseIntegrator.CloseFile(oldPath);
                FileSystemOperationsService.RenameFileOrDirectory(oldPath, newPath);
                if (closed)
                {
                    this.IseIntegrator.GoToFile(newPath);
                }
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
                this.DeleteTreeViewEntryItemModel(selectedItem);
                return;
            }
            var newPath = this.GenerateNewPath(selectedItem.Path, newValue);
            INode newNode = null;
            if (this.FindTreeViewEntryItemByPath(newPath) != null)
            {
                this.DocumentHierarchyFactory.RemoveTemporaryNode(selectedItem.Node);
                this.DeleteTreeViewEntryItemModel(selectedItem);
                MessageBoxHelper.ShowError("Item '" + newPath + "' already exists.");
                return;
            }
            if (selectedItem.NodeType == NodeType.Directory)
            {
                try
                {
                    newNode = this.DocumentHierarchyFactory.UpdateTemporaryNode(selectedItem.Node, newPath);
                    var parent = selectedItem.Parent;
                    this.DeleteTreeViewEntryItemModel(selectedItem);
                    selectedItem = this.CreateTreeViewEntryItemModel(newNode, parent, true);
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
                        this.DeleteTreeViewEntryItemModel(selectedItem);
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
                    this.DeleteTreeViewEntryItemModel(selectedItem);
                    selectedItem = this.CreateTreeViewEntryItemModel(newNode, parent, true);
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
                        this.DeleteTreeViewEntryItemModel(selectedItem);
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

        private bool HandleUnsavedFileManipulation(TreeViewEntryItemModel selectedItem)
        {
            if (selectedItem.NodeType == NodeType.File && this.IseIntegrator.OpenFiles.Contains(selectedItem.Path) && !this.IseIntegrator.IsFileSaved(selectedItem.Path))
            {
                this.IseIntegrator.GoToFile(selectedItem.Path);
                MessageBoxHelper.ShowInfo("Please save your changes or close the file first.");
                return false;
            }
            return true;
        }

        private TreeViewEntryItemModel CreateTreeViewEntryItemModel(INode node, TreeViewEntryItemModel parent, bool isSelected)
        {
            var lockObject = parent == null ? TreeViewEntryItemModel.RootLockObject : parent;
            lock (lockObject)
            {
                var item = new TreeViewEntryItemModel(node, parent, isSelected);
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

        private void DeleteTreeViewEntryItemModel(TreeViewEntryItemModel item, bool first = true)
        {
            if (item == this.RootTreeViewEntryItem)
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
