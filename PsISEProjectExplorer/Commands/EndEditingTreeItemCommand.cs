using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using PsISEProjectExplorer.Services;
using PsISEProjectExplorer.UI.Helpers;
using PsISEProjectExplorer.UI.IseIntegration;
using PsISEProjectExplorer.UI.ViewModel;
using System;

namespace PsISEProjectExplorer.Commands
{
    [Component]
    public class EndEditingTreeItemCommand : ParameterizedCommand<Tuple<string, bool>>
    {
        private readonly MainViewModel mainViewModel;

        private readonly TreeViewModel treeViewModel;

        private readonly FilesPatternProvider filesPatternProvider;

        private readonly IseIntegrator iseIntegrator;

        private readonly FileSystemOperationsService fileSystemOperationsService;

        private readonly MessageBoxHelper messageBoxHelper;

        private readonly DocumentHierarchyFactory documentHierarchyFactory;

        public EndEditingTreeItemCommand(MainViewModel mainViewModel, TreeViewModel treeViewModel, FilesPatternProvider filesPatternProvider,
            IseIntegrator iseIntegrator, FileSystemOperationsService fileSystemOperationsService, MessageBoxHelper messageBoxHelper,
            DocumentHierarchyFactory documentHierarchyFactory)
        {
            this.mainViewModel = mainViewModel;
            this.treeViewModel = treeViewModel;
            this.filesPatternProvider = filesPatternProvider;
            this.iseIntegrator = iseIntegrator;
            this.fileSystemOperationsService = fileSystemOperationsService;
            this.messageBoxHelper = messageBoxHelper;
            this.documentHierarchyFactory = documentHierarchyFactory;
        }

        public void Execute(Tuple<string, bool> param)
        {
            string newValue = param.Item1;
            bool save = param.Item2;
            var selectedItem = this.treeViewModel.SelectedItem;
            if (selectedItem == null)
            {
                return;
            }

            var addFileExtension = !this.mainViewModel.SearchInFiles;
            selectedItem.IsBeingEdited = false;

            if (selectedItem.NodeType == NodeType.File && addFileExtension && !String.IsNullOrEmpty(newValue) && !this.filesPatternProvider.DoesFileMatch(newValue))
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
            if (!save || String.IsNullOrEmpty(newValue) || selectedItem == null)
            {
                return;
            }

            try
            {
                string oldPath = selectedItem.Path;
                string newPath = this.GenerateNewPath(selectedItem.Path, newValue);
                bool closed = this.iseIntegrator.CloseFile(oldPath);
                fileSystemOperationsService.RenameFileOrDirectory(oldPath, newPath);
                if (closed)
                {
                    this.iseIntegrator.GoToFile(newPath);
                }
            }
            catch (Exception e)
            {
                this.treeViewModel.PathOfItemToSelectOnRefresh = null;
                this.messageBoxHelper.ShowError("Failed to rename: " + e.Message);
            }

        }

        private void EndAddingTreeItem(string newValue, bool save, TreeViewEntryItemModel selectedItem)
        {
            if (selectedItem == null)
            {
                return;
            }
            if (!save || String.IsNullOrEmpty(newValue))
            {
                this.documentHierarchyFactory.RemoveTemporaryNode(selectedItem.Node);
                this.treeViewModel.DeleteTreeViewEntryItemModel(selectedItem);
                return;
            }
            var newPath = this.GenerateNewPath(selectedItem.Path, newValue);
            INode newNode = null;
            if (this.treeViewModel.FindTreeViewEntryItemByPath(newPath) != null)
            {
                this.documentHierarchyFactory.RemoveTemporaryNode(selectedItem.Node);
                this.treeViewModel.DeleteTreeViewEntryItemModel(selectedItem);
                this.messageBoxHelper.ShowError("Item '" + newPath + "' already exists.");
                return;
            }
            if (selectedItem.NodeType == NodeType.Directory)
            {
                try
                {
                    newNode = this.documentHierarchyFactory.UpdateTemporaryNode(selectedItem.Node, newPath);
                    var parent = selectedItem.Parent;
                    this.treeViewModel.DeleteTreeViewEntryItemModel(selectedItem);
                    selectedItem = this.treeViewModel.CreateTreeViewEntryItemModel(newNode, parent, true);
                    this.filesPatternProvider.AddAdditionalPath(newPath);
                    fileSystemOperationsService.CreateDirectory(newPath);
                }
                catch (Exception e)
                {
                    if (newNode != null)
                    {
                        newNode.Remove();
                    }
                    if (selectedItem != null)
                    {
                        this.treeViewModel.DeleteTreeViewEntryItemModel(selectedItem);
                    }
                    this.treeViewModel.PathOfItemToSelectOnRefresh = null;
                    this.messageBoxHelper.ShowError("Failed to create directory '" + newPath + "': " + e.Message);
                }
            }
            else if (selectedItem.NodeType == NodeType.File)
            {
                try
                {
                    newNode = this.documentHierarchyFactory.UpdateTemporaryNode(selectedItem.Node, newPath);
                    var parent = selectedItem.Parent;
                    this.treeViewModel.DeleteTreeViewEntryItemModel(selectedItem);
                    selectedItem = this.treeViewModel.CreateTreeViewEntryItemModel(newNode, parent, true);
                    this.filesPatternProvider.AddAdditionalPath(newPath);
                    fileSystemOperationsService.CreateFile(newPath);
                    this.iseIntegrator.GoToFile(newPath);
                }
                catch (Exception e)
                {
                    if (newNode != null)
                    {
                        newNode.Remove();
                    }
                    if (selectedItem != null)
                    {
                        this.treeViewModel.DeleteTreeViewEntryItemModel(selectedItem);
                    }
                    this.treeViewModel.PathOfItemToSelectOnRefresh = null;
                    this.messageBoxHelper.ShowError("Failed to create file '" + newPath + "': " + e.Message);
                }
            }
        }

        private string GenerateNewPath(string currentPath, string newValue)
        {
            var newPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(currentPath), newValue);
            this.treeViewModel.PathOfItemToSelectOnRefresh = newPath;
            return newPath;
        }

    }
}
