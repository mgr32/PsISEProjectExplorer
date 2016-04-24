using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using PsISEProjectExplorer.Services;
using PsISEProjectExplorer.UI.Helpers;
using PsISEProjectExplorer.UI.IseIntegration;
using PsISEProjectExplorer.UI.ViewModel;
using System;

namespace PsISEProjectExplorer.Commands
{
    public class EndEditingTreeItemCommand : ParameterizedCommand<Tuple<string, bool>>
    {

        private MainViewModel MainViewModel { get; set; }

        private TreeViewModel TreeViewModel { get; set; }

        private FilesPatternProvider FilesPatternProvider { get; set; }

        private IseIntegrator IseIntegrator { get; set; }

        private FileSystemOperationsService FileSystemOperationsService { get; set; }

        private MessageBoxHelper MessageBoxHelper { get; set; }

        private DocumentHierarchyFactory DocumentHierarchyFactory { get; set; }

        public EndEditingTreeItemCommand(MainViewModel mainViewModel, TreeViewModel treeViewModel, FilesPatternProvider filesPatternProvider,
            IseIntegrator iseIntegrator, FileSystemOperationsService fileSystemOperationsService, MessageBoxHelper messageBoxHelper,
            DocumentHierarchyFactory documentHierarchyFactory)
        {
            this.MainViewModel = mainViewModel;
            this.TreeViewModel = treeViewModel;
            this.FilesPatternProvider = filesPatternProvider;
            this.IseIntegrator = iseIntegrator;
            this.FileSystemOperationsService = fileSystemOperationsService;
            this.MessageBoxHelper = messageBoxHelper;
            this.DocumentHierarchyFactory = documentHierarchyFactory;
        }

        public void Execute(Tuple<string, bool> param)
        {
            string newValue = param.Item1;
            bool save = param.Item2;
            var selectedItem = this.TreeViewModel.SelectedItem;
            if (selectedItem == null)
            {
                return;
            }

            var addFileExtension = !this.MainViewModel.SearchInFiles;
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
            if (!save || String.IsNullOrEmpty(newValue) || selectedItem == null)
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
                this.TreeViewModel.PathOfItemToSelectOnRefresh = null;
                this.MessageBoxHelper.ShowError("Failed to rename: " + e.Message);
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
                this.DocumentHierarchyFactory.RemoveTemporaryNode(selectedItem.Node);
                this.TreeViewModel.DeleteTreeViewEntryItemModel(selectedItem);
                return;
            }
            var newPath = this.GenerateNewPath(selectedItem.Path, newValue);
            INode newNode = null;
            if (this.TreeViewModel.FindTreeViewEntryItemByPath(newPath) != null)
            {
                this.DocumentHierarchyFactory.RemoveTemporaryNode(selectedItem.Node);
                this.TreeViewModel.DeleteTreeViewEntryItemModel(selectedItem);
                this.MessageBoxHelper.ShowError("Item '" + newPath + "' already exists.");
                return;
            }
            if (selectedItem.NodeType == NodeType.Directory)
            {
                try
                {
                    newNode = this.DocumentHierarchyFactory.UpdateTemporaryNode(selectedItem.Node, newPath);
                    var parent = selectedItem.Parent;
                    this.TreeViewModel.DeleteTreeViewEntryItemModel(selectedItem);
                    selectedItem = this.TreeViewModel.CreateTreeViewEntryItemModel(newNode, parent, true);
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
                        this.TreeViewModel.DeleteTreeViewEntryItemModel(selectedItem);
                    }
                    this.TreeViewModel.PathOfItemToSelectOnRefresh = null;
                    this.MessageBoxHelper.ShowError("Failed to create directory '" + newPath + "': " + e.Message);
                }
            }
            else if (selectedItem.NodeType == NodeType.File)
            {
                try
                {
                    newNode = this.DocumentHierarchyFactory.UpdateTemporaryNode(selectedItem.Node, newPath);
                    var parent = selectedItem.Parent;
                    this.TreeViewModel.DeleteTreeViewEntryItemModel(selectedItem);
                    selectedItem = this.TreeViewModel.CreateTreeViewEntryItemModel(newNode, parent, true);
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
                        this.TreeViewModel.DeleteTreeViewEntryItemModel(selectedItem);
                    }
                    this.TreeViewModel.PathOfItemToSelectOnRefresh = null;
                    this.MessageBoxHelper.ShowError("Failed to create file '" + newPath + "': " + e.Message);
                }
            }
        }

        private string GenerateNewPath(string currentPath, string newValue)
        {
            var newPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(currentPath), newValue);
            this.TreeViewModel.PathOfItemToSelectOnRefresh = newPath;
            return newPath;
        }

    }
}
