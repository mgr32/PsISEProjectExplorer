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

        private static readonly string NOITEMS_NOSEARCH = "No files to show. Please enable 'Show All Files' or change workspace directory.";

        private static readonly string NOITEMS_SEARCH = "No items matching your query.";

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public TreeViewModel TreeViewModel { get; private set; }

        public WorkspaceDirectoryModel WorkspaceDirectoryModel { get; private set; }

        public IndexingSearchingModel IndexingSearchingModel { get; private set; }

        public event EventHandler<IseEventArgs> ActiveDocumentSyncEvent;

        private string searchText;

        public string SearchText
        {
            get { return this.searchText; }
            set
            {
                this.searchText = value;
                Logger.Debug("Search text changed to: " + this.searchText);
                this.OnPropertyChanged();
                this.OnPropertyChanged("NoTreeItemsString");
                this.RunSearch();
            }
        }

        public string NoTreeItemsString
        {
            get
            {
                return String.IsNullOrEmpty(this.SearchText) ? NOITEMS_NOSEARCH : NOITEMS_SEARCH;
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
                ConfigHandler.SaveConfigValue("SyncWithActiveDocument", value.ToString());
            }
        }

        private bool isTreeViewReady;

        public bool IsTreeViewReady
        {
            get { return this.isTreeViewReady; }
            set
            {
                this.isTreeViewReady = value;
                this.OnPropertyChanged();
            }
        }

        private SearchOptions SearchOptions { get; set; }

        private DocumentHierarchyFactory DocumentHierarchyFactory { get; set; }

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
                this.IseFileReloader = new IseFileReloader(this.iseIntegrator);
                this.TreeViewModel.IseIntegrator = this.iseIntegrator;
                this.WorkspaceDirectoryModel.IseIntegrator = this.iseIntegrator;
                this.iseIntegrator.FileTabChanged += OnFileTabChanged;
                if (!this.WorkspaceDirectoryModel.ResetWorkspaceDirectoryIfRequired())
                {
                    this.ReindexSearchTree();
                }
            }
        }

        private IseFileReloader IseFileReloader { get; set; }

        private FilesPatternProvider FilesPatternProvider { get; set; }

        private FileSystemChangeWatcher FileSystemChangeWatcher { get; set; }

        private DocumentHierarchySearcher DocumentHierarchySearcher { get; set; }

        public MainViewModel()
        {
            this.searchInFiles = ConfigHandler.ReadConfigBoolValue("SearchInFiles", false);
            this.showAllFiles = ConfigHandler.ReadConfigBoolValue("ShowAllFiles", false);
            this.FilesPatternProvider = new FilesPatternProvider(this.showAllFiles);
            this.syncWithActiveDocument = ConfigHandler.ReadConfigBoolValue("SyncWithActiveDocument", false);
            var searchField = (this.searchInFiles ? FullTextFieldType.CatchAll : FullTextFieldType.Name);
            this.SearchOptions = new SearchOptions { IncludeAllParents = true, SearchField = searchField };

            this.DocumentHierarchyFactory = new DocumentHierarchyFactory();
            this.FileSystemChangeWatcher = new FileSystemChangeWatcher(this.ReindexOnFileSystemChanged);
            this.IndexingSearchingModel = new IndexingSearchingModel(this.OnSearchingFinished, this.OnIndexingFinished);
            this.TreeViewModel = new TreeViewModel(this.FileSystemChangeWatcher, this.DocumentHierarchyFactory, this.FilesPatternProvider);
            this.WorkspaceDirectoryModel = new WorkspaceDirectoryModel();
            if (this.WorkspaceDirectoryModel.CurrentWorkspaceDirectory != null)
            {
                this.DocumentHierarchySearcher = this.DocumentHierarchyFactory.CreateDocumentHierarchySearcher(this.WorkspaceDirectoryModel.CurrentWorkspaceDirectory);
            }
            this.WorkspaceDirectoryModel.PropertyChanged += this.OnWorkspaceDirectoryChanged;
        }

        public void GoToDefinition()
        {
            string funcName = this.GetFunctionNameAtCurrentPosition();
            if (funcName == null || this.DocumentHierarchySearcher == null)
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
            this.TreeViewModel.Clear();
            this.IsTreeViewReady = false;
            this.ReindexSearchTree(null);
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
                this.DocumentHierarchySearcher = this.DocumentHierarchyFactory.CreateDocumentHierarchySearcher(this.WorkspaceDirectoryModel.CurrentWorkspaceDirectory);
                this.ReindexSearchTree();
            }
        }

        private void ReindexSearchTree(IEnumerable<string> pathsChanged)
        {
            var indexerParams = new BackgroundIndexerParams(this.DocumentHierarchyFactory, this.WorkspaceDirectoryModel.CurrentWorkspaceDirectory, pathsChanged, this.FilesPatternProvider);
            this.IndexingSearchingModel.ReindexSearchTree(indexerParams);
        }

        private void RunSearch()
        {
            this.IsTreeViewReady = false;
            var searcherParams = new BackgroundSearcherParams(this.DocumentHierarchySearcher, this.SearchOptions, this.SearchText);
            this.IndexingSearchingModel.RunSearch(searcherParams);
        }

        private void OnSearchingFinished(object sender, SearcherResult result)
        {
            bool expandNewNodes = !String.IsNullOrWhiteSpace(this.SearchText);
            this.TreeViewModel.RefreshFromRoot(result.ResultNode, expandNewNodes, this.FilesPatternProvider);
            this.IsTreeViewReady = true;
            // when 'Sync with active document' is enabled and search results changed, we need to try to locate current document in the new search results
            this.ActiveDocumentPotentiallyChanged();
        }

        private void OnIndexingFinished(object sender, IndexerResult result)
        {
            this.RunSearch();
        }

    }
}
