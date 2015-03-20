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
	public class TreeViewModel : BaseViewModel
    {

        public IEnumerable<TreeViewEntryItemModel> TreeViewItems
        {
            get
            {
                if (RootTreeViewEntryItem == null)
                {
                    return new TreeViewEntryItemObservableSet();
                }
                return RootTreeViewEntryItem.Children;
            }
        }

        public IseIntegrator IseIntegrator { private get; set; }

        private TreeViewEntryItemModel rootTreeViewEntryItem;

        public TreeViewEntryItemModel RootTreeViewEntryItem
        {
            get
            {
                return rootTreeViewEntryItem;
            }
            private set
            {
				rootTreeViewEntryItem = value;
				OnPropertyChanged();
				OnPropertyChanged("TreeViewItems");
            }
        }

        private int numberOfFiles;

        public int NumberOfFiles
        { 
            get
            {
                return numberOfFiles;
            }
            private set
            {
				numberOfFiles = value;
				OnPropertyChanged();
            }   
        }

        private string PathOfItemToSelectOnRefresh { get; set; }

        private FileSystemChangeWatcher FileSystemChangeWatcher { get; set; }

        private DocumentHierarchyFactory DocumentHierarchyFactory { get; set; }

        private FilesPatternProvider FilesPatternProvider { get; set; }

        private IDictionary<string, TreeViewEntryItemModel> ItemsMap { get; set; }

        public TreeViewModel(FileSystemChangeWatcher fileSystemChangeWatcher, DocumentHierarchyFactory documentHierarchyFactory, FilesPatternProvider filesPatternProvider)
        {
			FileSystemChangeWatcher = fileSystemChangeWatcher;
			DocumentHierarchyFactory = documentHierarchyFactory;
			FilesPatternProvider = filesPatternProvider;
			ItemsMap = new Dictionary<string, TreeViewEntryItemModel>();
        }

        public void ReRoot(INode rootNode)
        {
            lock (TreeViewEntryItemModel.RootLockObject)
            {
				ItemsMap.Clear();
				RootTreeViewEntryItem = rootNode == null ? null : CreateTreeViewEntryItemModel(rootNode, null, false);
				NumberOfFiles = 0;
            }
        }

        public TreeViewEntryItemModel FindTreeViewEntryItemByPath(string path)
        {
            lock (TreeViewEntryItemModel.RootLockObject)
            {
                TreeViewEntryItemModel result;
				ItemsMap.TryGetValue(path, out result);
                return result;
            }
        }

        public void RefreshFromNode(INode node, string path, bool expandAllNodes)
        {
            // node == null -> search returned no results at all
            if (node == null)
            {
				HandleNoResultsFound(path);
                return;
            }
            TreeViewEntryItemModel treeViewEntryItem = FindTreeViewEntryItemByPath(node.Path);
            if (treeViewEntryItem == null)
            {
                bool isSelected = node.Path == PathOfItemToSelectOnRefresh;
                treeViewEntryItem = CreateTreeViewEntryItemModelWithNodeParents(node, isSelected, expandAllNodes);
            }
            else
            {
				UpdateNode(treeViewEntryItem, node);
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
			RefreshFromIntermediateNode(node, treeViewEntryItem, expandAllNodes);
			OnPropertyChanged("TreeViewItems");
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
				ReRoot(RootTreeViewEntryItem.Node);
                return;
            }

            // path != null -> search was run on a subtree and gave no results -> remove the item
            var item = FindTreeViewEntryItemByPath(path);
            if (item != null)
            {
				DeleteTreeViewEntryItemModel(item);
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
				DeleteTreeViewEntryItemModel(item);
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
                        bool isSelected = docHierarchyChild.Path == PathOfItemToSelectOnRefresh;
                        newTreeViewItem = CreateTreeViewEntryItemModel(docHierarchyChild, treeViewEntryItem, isSelected);
                    }
                    else
                    {
						UpdateNode(newTreeViewItem, docHierarchyChild);
                    }
                    if (expandAllNodes)
                    {
                        newTreeViewItem.IsExpanded = true;
                    }
                }
				RefreshFromIntermediateNode(docHierarchyChild, newTreeViewItem, expandAllNodes);
            }
        }

        public void OpenItem(TreeViewEntryItemModel item, string searchText)
        {
            if (IseIntegrator == null)
            {
                throw new InvalidOperationException("IseIntegrator has not ben set yet.");
            }
            if (item == null)
            {
                return;
            }

            if (item.Node.NodeType == NodeType.File)
            {
                bool wasOpen = (IseIntegrator.SelectedFilePath == item.Node.Path);
                if (!wasOpen)
                {
					IseIntegrator.GoToFile(item.Node.Path);
                }
                else
                {
					IseIntegrator.SetFocusOnCurrentTab();
                }
                if (searchText != null && searchText.Length > 2)
                {
                    EditorInfo editorInfo = (wasOpen ? IseIntegrator.GetCurrentLineWithColumnIndex() : null);
                    TokenPosition tokenPos = TokenLocator.LocateNextToken(item.Node.Path, searchText, editorInfo);
                    if (tokenPos.MatchLength > 2)
                    {
						IseIntegrator.SelectText(tokenPos.Line, tokenPos.Column, tokenPos.MatchLength);
                    }
                    else if (string.IsNullOrEmpty(IseIntegrator.SelectedText))
                    {
                        tokenPos = TokenLocator.LocateSubtoken(item.Node.Path, searchText);
                        if (tokenPos.MatchLength > 2)
                        {
							IseIntegrator.SelectText(tokenPos.Line, tokenPos.Column, tokenPos.MatchLength);
                        }
                    }
                }
            }
            else if (item.Node.NodeType == NodeType.Function)
            {
                var node = ((PowershellItemNode)item.Node);
				IseIntegrator.GoToFile(node.FilePath);
				IseIntegrator.SelectText(node.PowershellItem.StartLine, node.PowershellItem.StartColumn, node.Name.Length);
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
            if (!HandleUnsavedFileManipulation(selectedItem))
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
					IseIntegrator.CloseFile(selectedItem.Path);
					FilesPatternProvider.RemoveAdditionalPath(selectedItem.Path);
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
            if (DocumentHierarchyFactory == null)
            {
                return;
            }
            if (parent == null)
            {
                parent = RootTreeViewEntryItem;
            }
            parent.IsExpanded = true;
            INode newNode = DocumentHierarchyFactory.CreateTemporaryNode(parent.Node, nodeType);
            if (newNode == null)
            {
                return;
            }
            var newItem = CreateTreeViewEntryItemModel(newNode, parent, true);
            newItem.IsBeingEdited = true;
            newItem.IsBeingAdded = true;
        }

        public void StartEditingTreeItem(TreeViewEntryItemModel item)
        {
            if (!HandleUnsavedFileManipulation(item))
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
            if (!HandleUnsavedFileManipulation(movedItem))
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
                    newPath = GenerateNewPathForDir(rootDirectory, movedItem.Name);
                }
                else if (destinationItem.NodeType == NodeType.File)
                {
                    newPath = GenerateNewPath(destinationItem.Path, movedItem.Name);
                }
                else if (destinationItem.NodeType == NodeType.Directory)
                {
                    newPath = GenerateNewPathForDir(destinationItem.Path, movedItem.Name);
                }
                else
                {
                    return;
                }
				FilesPatternProvider.RemoveAdditionalPath(movedItem.Path);
				FilesPatternProvider.AddAdditionalPath(newPath);
                bool closed = IseIntegrator.CloseFile(movedItem.Path);
                FileSystemOperationsService.RenameFileOrDirectory(movedItem.Path, newPath);
                if (closed)
                {
					IseIntegrator.GoToFile(newPath);
                }
                if (destinationItem != null)
                {
                    destinationItem.IsExpanded = true;
                }
            }
            catch (Exception e)
            {
				PathOfItemToSelectOnRefresh = null;
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

            if (selectedItem.NodeType == NodeType.File && addFileExtension && !String.IsNullOrEmpty(newValue) && !FilesPatternProvider.DoesFileMatch(newValue))
            {
                newValue += ".ps1";
            }
            if (selectedItem.IsBeingAdded)
            {
                selectedItem.IsBeingAdded = false;
				EndAddingTreeItem(newValue, save, selectedItem);
            }
            else
            {
				EndRenamingTreeItem(newValue, save, selectedItem);
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
                string newPath = GenerateNewPath(selectedItem.Path, newValue);
                bool closed = IseIntegrator.CloseFile(oldPath);
                FileSystemOperationsService.RenameFileOrDirectory(oldPath, newPath);
                if (closed)
                {
					IseIntegrator.GoToFile(newPath);
                }
            }
            catch (Exception e)
            {
				PathOfItemToSelectOnRefresh = null;
                MessageBoxHelper.ShowError("Failed to rename: " + e.Message);
            }

        }

        private void EndAddingTreeItem(string newValue, bool save, TreeViewEntryItemModel selectedItem)
        {
            if (!save || String.IsNullOrEmpty(newValue))
            {
				DocumentHierarchyFactory.RemoveTemporaryNode(selectedItem.Node);
				DeleteTreeViewEntryItemModel(selectedItem);
                return;
            }
            var newPath = GenerateNewPath(selectedItem.Path, newValue);
            INode newNode = null;
            if (FindTreeViewEntryItemByPath(newPath) != null)
            {
				DocumentHierarchyFactory.RemoveTemporaryNode(selectedItem.Node);
				DeleteTreeViewEntryItemModel(selectedItem);
                MessageBoxHelper.ShowError("Item '" + newPath + "' already exists.");
                return;
            }
            if (selectedItem.NodeType == NodeType.Directory)
            {
                try
                {
                    newNode = DocumentHierarchyFactory.UpdateTemporaryNode(selectedItem.Node, newPath);
                    var parent = selectedItem.Parent;
					DeleteTreeViewEntryItemModel(selectedItem);
                    selectedItem = CreateTreeViewEntryItemModel(newNode, parent, true);
					FilesPatternProvider.AddAdditionalPath(newPath);
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
						DeleteTreeViewEntryItemModel(selectedItem);
                    }
					PathOfItemToSelectOnRefresh = null;
                    MessageBoxHelper.ShowError("Failed to create directory '" + newPath + "': " + e.Message);
                }
            }
            else if (selectedItem.NodeType == NodeType.File)
            {
                try
                {
                    newNode = DocumentHierarchyFactory.UpdateTemporaryNode(selectedItem.Node, newPath);
                    var parent = selectedItem.Parent;
					DeleteTreeViewEntryItemModel(selectedItem);
                    selectedItem = CreateTreeViewEntryItemModel(newNode, parent, true);
					FilesPatternProvider.AddAdditionalPath(newPath);
                    FileSystemOperationsService.CreateFile(newPath);
					IseIntegrator.GoToFile(newPath);
                }
                catch (Exception e)
                {
                    if (newNode != null)
                    {
                        newNode.Remove();
                    }
                    if (selectedItem != null)
                    {
						DeleteTreeViewEntryItemModel(selectedItem);
                    }
					PathOfItemToSelectOnRefresh = null;
                    MessageBoxHelper.ShowError("Failed to create file '" + newPath + "': " + e.Message);
                }
            }
        }

        private string GenerateNewPath(string currentPath, string newValue)
        {
            var newPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(currentPath), newValue);
			PathOfItemToSelectOnRefresh = newPath;
            return newPath;
        }

        private string GenerateNewPathForDir(string currentPath, string newValue)
        {
            var newPath = System.IO.Path.Combine(currentPath, newValue);
			PathOfItemToSelectOnRefresh = newPath;
            return newPath;
        }

        private bool HandleUnsavedFileManipulation(TreeViewEntryItemModel selectedItem)
        {
            if (selectedItem.NodeType == NodeType.File && IseIntegrator.OpenFiles.Contains(selectedItem.Path) && !IseIntegrator.IsFileSaved(selectedItem.Path))
            {
				IseIntegrator.GoToFile(selectedItem.Path);
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
				ItemsMap[node.Path] = item;
                if (node.NodeType == NodeType.File)
                {
                    lock (TreeViewEntryItemModel.RootLockObject)
                    {
						NumberOfFiles++;
                    }
                }
                return item;
            }
        }

        private TreeViewEntryItemModel CreateTreeViewEntryItemModelWithNodeParents(INode node, bool isSelected, bool expandAllNodes)
        {
            var itemParent = node.Parent == null ? null : CreateTreeViewEntryItemModelWithNodeParents(node.Parent, false, expandAllNodes);
            TreeViewEntryItemModel item = FindTreeViewEntryItemByPath(node.Path);
            if (item == null)
            {
                item = CreateTreeViewEntryItemModel(node, itemParent, isSelected);
            }
            if (expandAllNodes)
            {
                item.IsExpanded = true;
            }
            return item;
        }       

        private void DeleteTreeViewEntryItemModel(TreeViewEntryItemModel item, bool first = true)
        {
            if (item == RootTreeViewEntryItem)
            {
                return;
            }
            var lockObject = item.Parent == null ? TreeViewEntryItemModel.RootLockObject : item.Parent;
            IList<TreeViewEntryItemModel> children;
            lock (lockObject)
            {
				ItemsMap.Remove(item.Path);
                if (item.NodeType == NodeType.File)
                {
                    lock (TreeViewEntryItemModel.RootLockObject)
                    {
						NumberOfFiles--;
                    }
                }
                children = new List<TreeViewEntryItemModel>(item.Children);
            }
            foreach (var child in children)
            {
				DeleteTreeViewEntryItemModel(child, false);
            }
            if (first)
            {
                item.Delete();
            }
        }
    }
}
