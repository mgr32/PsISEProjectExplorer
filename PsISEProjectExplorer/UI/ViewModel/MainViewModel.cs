using Microsoft.PowerShell.Host.ISE;
using NLog;
using PsISEProjectExplorer.Config;
using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.Model;
using PsISEProjectExplorer.Model.DocHierarchy;
using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using PsISEProjectExplorer.Services;
using PsISEProjectExplorer.UI.IseIntegration;
using PsISEProjectExplorer.UI.Workers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace PsISEProjectExplorer.UI.ViewModel
{
    [Component]
    public class MainViewModel : BaseViewModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public TreeViewModel TreeViewModel { get; private set; }

        public WorkspaceDirectoryModel WorkspaceDirectoryModel { get; private set; }

        public IndexingSearchingModel IndexingSearchingModel { get; private set; }

        public event EventHandler<IseEventArgs> ActiveDocumentSyncEvent;

        public string SearchText
        {
            get { return this.SearchOptions.SearchText; }
            set
            {
                this.SearchOptions.SearchText = value;
                Logger.Debug("Search text changed to: " + this.SearchOptions.SearchText);
                this.OnPropertyChanged();
                this.RunSearch();
            }
        }

        public string TreeItemsResultString
        {
            get
            {
                return String.Format("Found {0} files{1}", this.TreeViewModel.NumberOfFiles,
                    this.NumOfIndexingThreads > 0 ? ", indexing in progress..." :
                    this.NumOfSearchingThreads > 0 ? ", searching in progress..." : ".");
            }
        }

        private bool searchRegex;

        public bool SearchRegex
        {
            get { return this.searchRegex; }
            set
            {
                this.searchRegex = value;
                this.OnPropertyChanged();
                this.SearchOptions.SearchRegex = this.searchRegex;
                this.configHandler.SaveConfigValue("SearchRegex", value.ToString());
                this.DocumentHierarchy = this.DocumentHierarchyFactory.CreateDocumentHierarchy(this.WorkspaceDirectoryModel.CurrentWorkspaceDirectory, this.AnalyzeDocumentContents);
                this.ReindexSearchTree();
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
                this.configHandler.SaveConfigValue("SearchInFiles", value.ToString());
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
                this.configHandler.SaveConfigValue("ShowAllFiles", value.ToString());
                this.ReindexSearchTree();
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
                this.configHandler.SaveConfigValue("SyncWithActiveDocument", value.ToString());
            }
        }

        private int numOfSearchingThreads;

        private int NumOfSearchingThreads
        {
            get
            {
                return this.numOfSearchingThreads;
            }
            set
            {
                this.numOfSearchingThreads = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged("TreeItemsResultString");
            }
        }

        private int numOfIndexingThreads;

        private int NumOfIndexingThreads
        {
            get
            {
                return this.numOfIndexingThreads;
            }
            set
            {
                this.numOfIndexingThreads = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged("TreeItemsResultString");
            }
        }


        public SearchOptions SearchOptions { get; private set; }

        public IseIntegrator IseIntegrator { get; private set; }

        private DocumentHierarchyFactory DocumentHierarchyFactory { get; set; }

        private IseFileReloader IseFileReloader { get; set; }

        private FilesPatternProvider FilesPatternProvider { get; set; }

        private FileSystemChangeWatcher FileSystemChangeWatcher { get; set; }

        private DocumentHierarchy DocumentHierarchy { get; set; }

        private PowershellTokenizerProvider PowershellTokenizerProvider { get; set; }

        private bool AnalyzeDocumentContents
        {
            get
            {
                return !this.SearchRegex;
            }
        }

        private readonly ConfigHandler configHandler;

        public MainViewModel(ConfigHandler configHandler, WorkspaceDirectoryModel workspaceDirectoryModel, DocumentHierarchyFactory documentHierarchyFactory,
            PowershellTokenizerProvider powershellTokenizerProvider, FileSystemChangeWatcher fileSystemChangeWatcher, IndexingSearchingModel indexingSearchingModel,
            TreeViewModel treeViewModel, FilesPatternProvider filesPatternProvider, IseIntegrator iseIntegrator, IseFileReloader iseFileReloader)
        {
            this.configHandler = configHandler;
            this.WorkspaceDirectoryModel = workspaceDirectoryModel;
            this.DocumentHierarchyFactory = documentHierarchyFactory;
            this.PowershellTokenizerProvider = powershellTokenizerProvider;
            this.FileSystemChangeWatcher = fileSystemChangeWatcher;
            this.IndexingSearchingModel = indexingSearchingModel;
            this.TreeViewModel = treeViewModel;
            this.FilesPatternProvider = filesPatternProvider;
            this.IseIntegrator = iseIntegrator;
            this.IseFileReloader = iseFileReloader;
            this.TreeViewModel.PropertyChanged += (s, e) => { if (e.PropertyName == "NumberOfFiles") this.OnPropertyChanged("TreeItemsResultString"); };


            fileSystemChangeWatcher.RegisterOnChangeCallback(this.ReindexOnFileSystemChanged);
            indexingSearchingModel.RegisterHandlers(this.OnSearchingFinished, this.OnIndexingFinished, this.OnIndexingProgress);

            this.searchRegex = configHandler.ReadConfigBoolValue("SearchRegex", false);
            this.searchInFiles = configHandler.ReadConfigBoolValue("SearchInFiles", true);
            this.showAllFiles = configHandler.ReadConfigBoolValue("ShowAllFiles", true);
            IEnumerable<string> excludePaths = configHandler.ReadConfigStringEnumerableValue("ExcludePaths");
            this.FilesPatternProvider.IncludeAllFiles = this.showAllFiles;
            this.FilesPatternProvider.ExcludePaths = excludePaths;
            
            this.syncWithActiveDocument = configHandler.ReadConfigBoolValue("SyncWithActiveDocument", false);
            var searchField = (this.searchInFiles ? FullTextFieldType.CatchAll : FullTextFieldType.Name);
            this.SearchOptions = new SearchOptions(searchField, string.Empty, this.searchRegex);

            if (this.WorkspaceDirectoryModel.CurrentWorkspaceDirectory != null)
            {
                this.DocumentHierarchy = this.DocumentHierarchyFactory.CreateDocumentHierarchy(this.WorkspaceDirectoryModel.CurrentWorkspaceDirectory, this.AnalyzeDocumentContents);
            }
            this.WorkspaceDirectoryModel.PropertyChanged += this.OnWorkspaceDirectoryChanged;

        }

        public void setIseHostObject(ObjectModelRoot hostObject)
        {
            this.IseIntegrator.setHostObject(hostObject);
            this.IseIntegrator.FileTabChanged += OnFileTabChanged;
            this.IseFileReloader.startWatching();
            if (!this.WorkspaceDirectoryModel.ResetWorkspaceDirectoryIfRequired())
            {
                this.ReindexSearchTree();
            }
        }

 

        public void ExcludeOrIncludeItem(TreeViewEntryItemModel selectedItem)
        {
            if (selectedItem == null)
            {
                return;
            }
            if (selectedItem.IsExcluded)
            {
                this.FilesPatternProvider.ExcludePaths = this.configHandler.RemoveConfigEnumerableValue("ExcludePaths", selectedItem.Path);
            }
            else
            {
                this.FilesPatternProvider.ExcludePaths = this.configHandler.AddConfigEnumerableValue("ExcludePaths", selectedItem.Path);
            }
            this.OnPropertyChanged();
            this.ReindexSearchTree();
        }
     
        private void ReindexOnFileSystemChanged(object sender, FileSystemChangedInfo changedInfo)
        {
            var workspaceDirectory = this.WorkspaceDirectoryModel.CurrentWorkspaceDirectory;
            var pathsChanged = changedInfo.PathsChanged.Where(p => p.RootPath == workspaceDirectory).Select(p => p.PathChanged).ToList();
            if (!pathsChanged.Any())
            {
                return;
            }
            if (pathsChanged.Contains(workspaceDirectory, StringComparer.InvariantCultureIgnoreCase))
            {
                pathsChanged = null;
            }
            Logger.Debug("OnFileSystemChanged: " + (pathsChanged == null ? "root" : string.Join(",", pathsChanged)));
            Application.Current.Dispatcher.Invoke(() => this.ReindexSearchTree(pathsChanged));
        }

        private void OnFileTabChanged(object sender, IseEventArgs args)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.WorkspaceDirectoryModel.ResetWorkspaceDirectoryIfRequired();
            });
            this.ActiveDocumentPotentiallyChanged();
        }

        public void ReindexSearchTree()
        {
            lock (this)
            {
                this.ClearTreeView();
                this.ReindexSearchTree(null);
            }
        }

        private void ClearTreeView()
        {
            var rootNode = this.DocumentHierarchy == null ? null : this.DocumentHierarchy.RootNode;
            this.TreeViewModel.ReRoot(rootNode);
            this.FileSystemChangeWatcher.StopWatching();
            if (rootNode != null)
            {
                this.FileSystemChangeWatcher.Watch(rootNode.Path, this.FilesPatternProvider);
            }
        }

        private void ActiveDocumentPotentiallyChanged()
        {
            if (this.ActiveDocumentSyncEvent != null)
            {
                this.ActiveDocumentSyncEvent(this, new IseEventArgs());
            }
        }

        private void OnWorkspaceDirectoryChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentWorkspaceDirectory")
            {
                this.DocumentHierarchy = this.DocumentHierarchyFactory.CreateDocumentHierarchy(this.WorkspaceDirectoryModel.CurrentWorkspaceDirectory, this.AnalyzeDocumentContents);
                this.ReindexSearchTree();
            }
        }

        // running in UI thread
        private void ReindexSearchTree(IEnumerable<string> pathsChanged)
        {
            lock (this)
            {
                this.NumOfIndexingThreads++;
            }
            var indexerParams = new BackgroundIndexerParams(this.DocumentHierarchyFactory, this.WorkspaceDirectoryModel.CurrentWorkspaceDirectory, pathsChanged, this.FilesPatternProvider);
            this.IndexingSearchingModel.ReindexSearchTree(indexerParams);
        }

        // running in Indexing or UI thread
        private void RunSearch(string path = null)
        {
            lock (this)
            {
                this.NumOfSearchingThreads++;
            }
            if (path == null)
            {
                this.ClearTreeView();
            }
            var searcherParams = new BackgroundSearcherParams(this.DocumentHierarchy, this.SearchOptions, path);
            this.IndexingSearchingModel.RunSearch(searcherParams);
        }

        // running in Indexing or UI thread
        private void OnSearchingFinished(object sender, SearcherResult result)
        {
            try
            {
                if (result == null || result.SearchOptions == null || !result.SearchOptions.Equals(this.SearchOptions))
                {
                    // this means that the thread was cancelled or SearchOptions have been changed in the meantime, so we need to ignore the result.
                    return;
                }
                bool expandNewNodes = !String.IsNullOrWhiteSpace(this.SearchText);
                if (Application.Current != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        this.TreeViewModel.RefreshFromNode(result.ResultNode, result.Path, expandNewNodes);
                        // when 'Sync with active document' is enabled and search results changed, we need to try to locate current document in the new search results
                        this.ActiveDocumentPotentiallyChanged();
                    });
                }
            }
            finally
            {
                lock (this)
                {
                    this.NumOfSearchingThreads--;
                }
            }
        }

        // running in Indexing thread
        private void OnIndexingProgress(object sender, string path)
        {
            this.RunSearch(path);
        }

        // running in UI thread
        private void OnIndexingFinished(object sender, IndexerResult result)
        {
            lock (this)
            {
                this.NumOfIndexingThreads--;
            }
        }
    }
}
