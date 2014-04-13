using NLog;
using PsISEProjectExplorer.Config;
using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.Model;
using PsISEProjectExplorer.Model.DocHierarchy;
using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using PsISEProjectExplorer.Services;
using PsISEProjectExplorer.UI.Helpers;
using PsISEProjectExplorer.UI.IseIntegration;
using PsISEProjectExplorer.UI.Workers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace PsISEProjectExplorer.UI.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static string EmptyRootDir = "<Please open a Powershell file or click 'Change' button>";

        public TreeViewModel TreeViewModel { get; private set; }

        public event EventHandler<IseEventArgs> ActiveDocumentSyncEvent;

        private string rootDirectoryToSearch;

        public string RootDirectoryToSearch
        {
            get { return this.rootDirectoryToSearch; }
            private set
            {
                this.rootDirectoryToSearch = value;
                this.DocumentHierarchySearcher = null;
                this.OnPropertyChanged();
                this.OnPropertyChanged("RootDirectoryLabel");
                ConfigHandler.SaveConfigValue("RootDirectory", value);
           }
        }

        private bool indexingInProgress;

        public bool IndexingInProgress
        {
            get { return this.indexingInProgress; }
            set
            {
                this.indexingInProgress = value;
                this.OnPropertyChanged();

            }
        }

        private string searchText;

        public string SearchText
        {
            get { return this.searchText; }
            set
            {
                this.searchText = value;
                Logger.Debug("Search text changed to: " + this.searchText);
                this.OnPropertyChanged();
                this.RunSearch();
            }
        }

        private bool searchInFiles;

        public bool SearchInFiles
        {
            get { return this.searchInFiles; }
            set
            {
                this.searchInFiles = value;
                this.OnPropertyChanged();
                this.SearchOptions.SearchField = (this.searchInFiles ? FullTextFieldType.CatchAll : FullTextFieldType.Name);
                if (!String.IsNullOrEmpty(this.SearchText))
                {
                    this.RunSearch();
                }
                ConfigHandler.SaveConfigValue("SearchInFiles", value.ToString());
            }
        }

        private bool autoUpdateRootDirectory;

        public bool AutoUpdateRootDirectory
        {
            get { return this.autoUpdateRootDirectory; }
            set
            {
                this.autoUpdateRootDirectory = value;
                this.OnPropertyChanged();
                if (!this.autoUpdateRootDirectory)
                {
                    this.DocumentHierarchySearcher = null;
                    this.RecalculateRootDirectory(false);
                }
                ConfigHandler.SaveConfigValue("AutoUpdateRootDirectory", value.ToString());
            }
        }

        private bool showAllFiles;

        public bool ShowAllFiles
        {
            get { return this.showAllFiles; }
            set
            {
                this.showAllFiles = value;
                this.FilesPatternProvider.IncludeAllFiles = value;
                this.OnPropertyChanged();
                ConfigHandler.SaveConfigValue("ShowAllFiles", value.ToString());
                this.ReindexSearchTree(null);
            }
        }

        private bool syncWithActiveDocument;

        public bool SyncWithActiveDocument
        {
            get { return this.syncWithActiveDocument; }
            set
            {
                this.syncWithActiveDocument = value;
                this.OnPropertyChanged();
                this.ActiveDocumentPotentiallyChanged();
                ConfigHandler.SaveConfigValue("SyncWithActiveDocument", value.ToString());
            }
        }

        public string RootDirectoryLabel
        {
            get { return "Project root: " + (String.IsNullOrEmpty(this.RootDirectoryToSearch) ? EmptyRootDir : this.RootDirectoryToSearch); }
        }

        private bool SearchTreeInitialized { get; set; }

        private SearchOptions SearchOptions { get; set; }

        private BackgroundIndexer BackgroundIndexer { get; set; }

        private BackgroundSearcher BackgroundSearcher { get; set; }

        private DocumentHierarchySearcher DocumentHierarchySearcher { get; set; }

        private DocumentHierarchyFactory DocumentHierarchyFactory { get; set; }

        private DateTime LastSearchStartTime { get; set; }

        private DateTime LastIndexStartTime { get; set; }

        private IseIntegrator iseIntegrator;

        public IseIntegrator IseIntegrator
        {
            get
            {
                return this.iseIntegrator;
            }
            set
            {
                this.iseIntegrator = value;
                this.TreeViewModel.IseIntegrator = this.iseIntegrator;
                this.iseIntegrator.FileTabChanged += OnFileTabChanged;
                this.RecalculateRootDirectory(true);
            }
        }

        private FilesPatternProvider FilesPatternProvider { get; set; }

        public MainViewModel()
        {
            this.autoUpdateRootDirectory = ConfigHandler.ReadConfigBoolValue("AutoUpdateRootDirectory", true);
            this.searchInFiles = ConfigHandler.ReadConfigBoolValue("SearchInFiles", false);
            this.showAllFiles = ConfigHandler.ReadConfigBoolValue("ShowAllFiles", false);
            this.FilesPatternProvider = new FilesPatternProvider(this.showAllFiles);
            this.syncWithActiveDocument = ConfigHandler.ReadConfigBoolValue("SyncWithActiveDocument", false);
            var searchField = (this.searchInFiles ? FullTextFieldType.CatchAll : FullTextFieldType.Name);
            this.rootDirectoryToSearch = ConfigHandler.ReadConfigStringValue("RootDirectory");
            if (this.rootDirectoryToSearch == string.Empty || !Directory.Exists(this.rootDirectoryToSearch))
            { 
                this.rootDirectoryToSearch = null;
            }
            this.TreeViewModel = new TreeViewModel();
            this.SearchOptions = new SearchOptions { IncludeAllParents = true, SearchField = searchField };
            this.DocumentHierarchyFactory = new DocumentHierarchyFactory();
            FileSystemChangeNotifier.FileSystemChanged += OnFileSystemChanged;
        }

        public void GoToDefinition()
        {
            string funcName = this.GetFunctionNameAtCurrentPosition();
            if (funcName == null)
            {
                return;
            }
            var node = (PowershellFunctionNode)this.DocumentHierarchySearcher.GetFunctionNodeByName(funcName);
            if (node == null)
            {
                return;
            }
            this.IseIntegrator.GoToFile(node.FilePath);
            this.IseIntegrator.SetCursor(node.PowershellFunction.StartLine, node.PowershellFunction.StartColumn);
            
        }

        public void FindAllOccurrences()
        {
            string funcName = this.GetFunctionNameAtCurrentPosition();
            if (funcName == null)
            {
                return;
            }
            
            // TODO: this is hacky...
            this.searchText = string.Empty;
            this.SearchInFiles = true;
            this.SearchText = funcName;
        }

        public void FindInFiles()
        {
            this.searchText = string.Empty;
            this.SearchInFiles = true;
        }

        private string GetFunctionNameAtCurrentPosition()
        {
            if (this.DocumentHierarchySearcher == null)
            {
                return null;
            }
            EditorInfo editorInfo = this.IseIntegrator.GetCurrentLineWithColumnIndex();
            if (editorInfo == null)
            {
                return null;
            }
            string funcName = editorInfo.GetTokenFromCurrentPosition();
            return funcName;
        }


        private void BackgroundIndexerWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.IndexingInProgress = false;
            var result = (WorkerResult)e.Result;
            if (result == null ||  result.Result == null || result.StartTimestamp != this.LastIndexStartTime)
            {
                return;
            }
            Logger.Debug("Indexing ended, searchTreeInitialized: " + this.SearchTreeInitialized);
            this.DocumentHierarchySearcher = (DocumentHierarchySearcher)result.Result;
            if (!this.SearchTreeInitialized)
            {
                this.RunSearch();
                this.SearchTreeInitialized = true;
            }

        }

        private void BackgroundSearcherWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var result = (WorkerResult)e.Result;
            if (result == null || result.StartTimestamp != this.LastSearchStartTime)
            {
                return;
            }
            Logger.Debug("Searching ended");
            var rootNode = (INode)result.Result;
            bool expandNewNodes = !String.IsNullOrWhiteSpace(this.SearchText);
            this.TreeViewModel.RefreshFromRoot(rootNode, expandNewNodes, this.FilesPatternProvider);
            this.ActiveDocumentPotentiallyChanged();
        }

        private void OnFileSystemChanged(object sender, FileSystemChangedInfo changedInfo)
        {
            var pathsChanged = changedInfo.PathsChanged.ToList();
            if (pathsChanged.Contains(this.RootDirectoryToSearch, StringComparer.InvariantCultureIgnoreCase))
            {
                pathsChanged = null;
            }
            Logger.Debug("OnFileSystemChanged: " + (pathsChanged == null ? "root" : string.Join(",", pathsChanged)));
            Application.Current.Dispatcher.Invoke(() => this.ReindexSearchTree(pathsChanged));
        }

        public void ChangeRootDirectory(string newPath)
        {
            this.RootDirectoryToSearch = newPath;
            this.AutoUpdateRootDirectory = false;
        }

        private void RecalculateRootDirectory(bool alwaysReindex)
        {
            if (this.IseIntegrator.SelectedFilePath != null && (this.AutoUpdateRootDirectory || this.RootDirectoryToSearch == null))
            {
                string selectedFilePath = this.IseIntegrator.SelectedFilePath;
                string newRootDirectoryToSearch = RootDirectoryProvider.GetRootDirectoryToSearch(selectedFilePath);
                if (newRootDirectoryToSearch != null && (this.RootDirectoryToSearch == null || newRootDirectoryToSearch != this.RootDirectoryToSearch))
                {
                    this.RootDirectoryToSearch = newRootDirectoryToSearch;
                    this.ReindexSearchTree(null);
                }
                else if (alwaysReindex)
                {
                    this.ReindexSearchTree(null);
                }
            }
            else
            {
                this.ReindexSearchTree(null);
            }
        }

        private void ReindexSearchTree(IEnumerable<string> pathsChanged)
        {
            this.SearchTreeInitialized = false;
            var indexerParams = new BackgroundIndexerParams(this.DocumentHierarchyFactory, this.rootDirectoryToSearch, pathsChanged, this.FilesPatternProvider);
            this.IndexingInProgress = true;
            this.BackgroundIndexer = new BackgroundIndexer();
            this.LastIndexStartTime = this.BackgroundIndexer.StartTimestamp;
            this.BackgroundIndexer.RunWorkerCompleted += this.BackgroundIndexerWorkCompleted;
            this.BackgroundIndexer.RunWorkerAsync(indexerParams);
        }

        private void RunSearch()
        {
            var searcherParams = new BackgroundSearcherParams(this.DocumentHierarchySearcher, this.SearchOptions, this.SearchText);
            this.BackgroundSearcher = new BackgroundSearcher();
            this.LastSearchStartTime = this.BackgroundSearcher.StartTimestamp;
            this.BackgroundSearcher.RunWorkerCompleted += this.BackgroundSearcherWorkCompleted;
            this.BackgroundSearcher.RunWorkerAsync(searcherParams);
        }

        private void OnFileTabChanged(object sender, IseEventArgs args)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (this.AutoUpdateRootDirectory)
                {
                    this.RecalculateRootDirectory(false);
                }
            });
            this.ActiveDocumentPotentiallyChanged();
        }

        public void EndTreeEdit(string newValue, bool save, TreeViewEntryItemModel selectedItem)
        {
            if (selectedItem == null)
            {
                return;
            }
            selectedItem.IsBeingEdited = false;
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
                this.IseIntegrator.ReopenFileAfterRename(oldPath, newPath);
            }
            catch (Exception e)
            {
                this.TreeViewModel.PathOfItemToSelectOnRefresh = null;
                MessageBoxHelper.ShowError("Failed to rename: " + e.Message);
            }
            
        }

        private void EndAddingTreeItem(string newValue, bool save, TreeViewEntryItemModel selectedItem)
        {
            if (!save || String.IsNullOrEmpty(newValue))
            {
                selectedItem.Delete();
                return;
            }
            if (selectedItem.NodeType == NodeType.File && !this.ShowAllFiles && !this.FilesPatternProvider.DoesFileMatch(newValue))
            {
                newValue += ".ps1";
            }
            var newPath = this.GenerateNewPath(selectedItem.Path, newValue);
            INode newNode = null;
            if (this.TreeViewModel.FindTreeViewEntryItemByPath(newPath) != null)
            {
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
                    this.TreeViewModel.PathOfItemToSelectOnRefresh = null;
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
                    this.TreeViewModel.PathOfItemToSelectOnRefresh = null;
                    MessageBoxHelper.ShowError("Failed to create file '" + newPath + "': " + e.Message);
                }
            }
        }

        private string GenerateNewPath(string currentPath, string newValue)
        {
            var newPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(currentPath), newValue);
            this.TreeViewModel.PathOfItemToSelectOnRefresh = newPath;
            return newPath;
        }

        private string GenerateNewPathForDir(string currentPath, string newValue)
        {
            var newPath = System.IO.Path.Combine(currentPath, newValue);
            this.TreeViewModel.PathOfItemToSelectOnRefresh = newPath;
            return newPath;
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

        public void AddNewTreeItem(TreeViewEntryItemModel selectedItem, NodeType nodeType)
        {
            if (selectedItem == null || this.DocumentHierarchyFactory == null)
            {
                return;
            }
            selectedItem.IsExpanded = true;
            INode newNode = this.DocumentHierarchyFactory.CreateTemporaryNode(selectedItem.Node, nodeType);
            if (newNode == null)
            {
                return;
            }
            var newItem = new TreeViewEntryItemModel(newNode, selectedItem, true);
            newItem.IsBeingEdited = true;
            newItem.IsBeingAdded = true;
        }

        public void MoveTreeItem(TreeViewEntryItemModel movedItem, TreeViewEntryItemModel destinationItem)
        {
            if (movedItem == destinationItem)
            {
                return;
            }
            try
            {
                string newPath;
                if (destinationItem.NodeType == NodeType.File)
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
                destinationItem.IsExpanded = true;
            }
            catch (Exception e)
            {
                this.TreeViewModel.PathOfItemToSelectOnRefresh = null;
                MessageBoxHelper.ShowError("Failed to move: " + e.Message);
            }
        }

        private void ActiveDocumentPotentiallyChanged()
        {
            if (this.ActiveDocumentSyncEvent != null)
            {
                this.ActiveDocumentSyncEvent(this, new IseEventArgs());
            }
        }
    }
}
