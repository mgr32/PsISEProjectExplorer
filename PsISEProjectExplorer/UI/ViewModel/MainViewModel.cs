using NLog;
using PsISEProjectExplorer.Config;
using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.Model;
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
	public class MainViewModel : BaseViewModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public TreeViewModel TreeViewModel { get; private set; }

        public WorkspaceDirectoryModel WorkspaceDirectoryModel { get; private set; }

        public IndexingSearchingModel IndexingSearchingModel { get; private set; }

        public event EventHandler<IseEventArgs> ActiveDocumentSyncEvent;

        public string SearchText
        {
            get { return SearchOptions.SearchText; }
            set
            {
				SearchOptions.SearchText = value;
                Logger.Debug("Search text changed to: " + SearchOptions.SearchText);
				OnPropertyChanged();
				RunSearch();
            }
        }

        public string TreeItemsResultString
        {
            get
            {
                return String.Format("Found {0} files{1}", TreeViewModel.NumberOfFiles,
					NumOfIndexingThreads > 0 ? ", indexing in progress..." :
					NumOfSearchingThreads > 0 ? ", searching in progress..." : ".");
            }
        }

        private bool searchInFiles;

        public bool SearchInFiles
        {
            get { return searchInFiles; }
            set
            {
				searchInFiles = value;
				OnPropertyChanged();
				SearchOptions.SearchField = (searchInFiles ? FullTextFieldType.CatchAll : FullTextFieldType.Name);
                if (!String.IsNullOrEmpty(SearchText))
                {
					RunSearch();
                }
                ConfigHandler.SaveConfigValue("SearchInFiles", value.ToString());
            }
        }

        private bool showAllFiles;

        public bool ShowAllFiles
        {
            get { return showAllFiles; }
            set
            {
				showAllFiles = value;
				FilesPatternProvider.IncludeAllFiles = value;
				OnPropertyChanged();
                ConfigHandler.SaveConfigValue("ShowAllFiles", value.ToString());
				ReindexSearchTree();
            }
        }

        private bool syncWithActiveDocument;

        public bool SyncWithActiveDocument
        {
            get { return syncWithActiveDocument; }
            set
            {
				syncWithActiveDocument = value;
				OnPropertyChanged();
				ActiveDocumentPotentiallyChanged();
                ConfigHandler.SaveConfigValue("SyncWithActiveDocument", value.ToString());
            }
        }

        private int numOfSearchingThreads;

        private int NumOfSearchingThreads
        {
            get
            {
                return numOfSearchingThreads;
            }
            set
            {
				numOfSearchingThreads = value;
				OnPropertyChanged();
				OnPropertyChanged("TreeItemsResultString");
            }
        }

        private int numOfIndexingThreads;

        private int NumOfIndexingThreads
        {
            get
            {
                return numOfIndexingThreads;
            }
            set
            {
				numOfIndexingThreads = value;
				OnPropertyChanged();
				OnPropertyChanged("TreeItemsResultString");
            }
        }


        private SearchOptions SearchOptions { get; set; }

        private DocumentHierarchyFactory DocumentHierarchyFactory { get; set; }

        private IseIntegrator iseIntegrator;

        public IseIntegrator IseIntegrator
        {
            get
            {
                return iseIntegrator;
            }
            set
            {
				iseIntegrator = value;
				IseFileReloader = new IseFileReloader(iseIntegrator);
				TreeViewModel.IseIntegrator = iseIntegrator;
				WorkspaceDirectoryModel.IseIntegrator = iseIntegrator;
				iseIntegrator.FileTabChanged += OnFileTabChanged;
                if (!WorkspaceDirectoryModel.ResetWorkspaceDirectoryIfRequired())
                {
					ReindexSearchTree();
                }
            }
        }

        private IseFileReloader IseFileReloader { get; set; }

        private FilesPatternProvider FilesPatternProvider { get; set; }

        private FileSystemChangeWatcher FileSystemChangeWatcher { get; set; }

        private DocumentHierarchySearcher DocumentHierarchySearcher { get; set; }

        public MainViewModel()
        {
			searchInFiles = ConfigHandler.ReadConfigBoolValue("SearchInFiles", true);
			showAllFiles = ConfigHandler.ReadConfigBoolValue("ShowAllFiles", true);
			FilesPatternProvider = new FilesPatternProvider(showAllFiles);
			syncWithActiveDocument = ConfigHandler.ReadConfigBoolValue("SyncWithActiveDocument", false);
            var searchField = (searchInFiles ? FullTextFieldType.CatchAll : FullTextFieldType.Name);
			SearchOptions = new SearchOptions(searchField, string.Empty);

			DocumentHierarchyFactory = new DocumentHierarchyFactory();
			FileSystemChangeWatcher = new FileSystemChangeWatcher(ReindexOnFileSystemChanged);
			IndexingSearchingModel = new IndexingSearchingModel(OnSearchingFinished, OnIndexingFinished, OnIndexingProgress);
			TreeViewModel = new TreeViewModel(FileSystemChangeWatcher, DocumentHierarchyFactory, FilesPatternProvider);
			TreeViewModel.PropertyChanged += (s, e) => { if (e.PropertyName == "NumberOfFiles") OnPropertyChanged("TreeItemsResultString"); };
			WorkspaceDirectoryModel = new WorkspaceDirectoryModel();
            if (WorkspaceDirectoryModel.CurrentWorkspaceDirectory != null)
            {
				DocumentHierarchySearcher = DocumentHierarchyFactory.CreateDocumentHierarchySearcher(WorkspaceDirectoryModel.CurrentWorkspaceDirectory);
            }
			WorkspaceDirectoryModel.PropertyChanged += OnWorkspaceDirectoryChanged;
        }

        public void GoToDefinition()
        {
            string funcName = GetFunctionNameAtCurrentPosition();
            if (funcName == null || DocumentHierarchySearcher == null)
            {
                return;
            }
            var node = (PowershellItemNode)DocumentHierarchySearcher.GetFunctionNodeByName(funcName);
            if (node == null)
            {
                return;
            }
			IseIntegrator.GoToFile(node.FilePath);
			IseIntegrator.SetCursor(node.PowershellItem.StartLine, node.PowershellItem.StartColumn);
            
        }

        public void FindAllOccurrences()
        {
            string funcName = GetFunctionNameAtCurrentPosition();
            if (funcName == null)
            {
                return;
            }

			// TODO: this is hacky...
			SearchOptions.SearchText = string.Empty;
			SearchInFiles = true;
			SearchText = funcName;
        }

        public void FindInFiles()
        {
			SearchOptions.SearchText = string.Empty;
			SearchInFiles = true;
        }

        private string GetFunctionNameAtCurrentPosition()
        {
            if (DocumentHierarchySearcher == null)
            {
                return null;
            }
            EditorInfo editorInfo = IseIntegrator.GetCurrentLineWithColumnIndex();
            if (editorInfo == null)
            {
                return null;
            }
            string funcName = editorInfo.GetTokenFromCurrentPosition();
            return funcName;
        }

        private void ReindexOnFileSystemChanged(object sender, FileSystemChangedInfo changedInfo)
        {
            var workspaceDirectory = WorkspaceDirectoryModel.CurrentWorkspaceDirectory;
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
            Application.Current.Dispatcher.Invoke(() => ReindexSearchTree(pathsChanged));
        }

        private void OnFileTabChanged(object sender, IseEventArgs args)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
				WorkspaceDirectoryModel.ResetWorkspaceDirectoryIfRequired();
            });
			ActiveDocumentPotentiallyChanged();
        }
     
        public void ReindexSearchTree()
        {
            lock (this)
            {
				ClearTreeView();
				ReindexSearchTree(null);
            }
        }

        private void ClearTreeView()
        {
            var rootNode = DocumentHierarchySearcher == null ? null : DocumentHierarchySearcher.RootNode;
			TreeViewModel.ReRoot(rootNode);
			FileSystemChangeWatcher.StopWatching();
            if (rootNode != null)
            {
				FileSystemChangeWatcher.Watch(rootNode.Path, FilesPatternProvider);
            }
        }

        private void ActiveDocumentPotentiallyChanged()
        {
            if (ActiveDocumentSyncEvent != null)
            {
				ActiveDocumentSyncEvent(this, new IseEventArgs());
            }
        }

        private void OnWorkspaceDirectoryChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentWorkspaceDirectory")
            {
				DocumentHierarchySearcher = DocumentHierarchyFactory.CreateDocumentHierarchySearcher(WorkspaceDirectoryModel.CurrentWorkspaceDirectory);
				ReindexSearchTree();
            }
        }

        // running in UI thread
        private void ReindexSearchTree(IEnumerable<string> pathsChanged)
        {
            lock (this)
            {
				NumOfIndexingThreads++;
            }
            var indexerParams = new BackgroundIndexerParams(DocumentHierarchyFactory, WorkspaceDirectoryModel.CurrentWorkspaceDirectory, pathsChanged, FilesPatternProvider);
			IndexingSearchingModel.ReindexSearchTree(indexerParams);
        }

        // running in Indexing or UI thread
        private void RunSearch(string path = null)
        {
            lock (this)
            {
				NumOfSearchingThreads++;
            }
            if (path == null)
            {
				ClearTreeView();
            }
            var searcherParams = new BackgroundSearcherParams(DocumentHierarchySearcher, SearchOptions, path);
			IndexingSearchingModel.RunSearch(searcherParams);
        }

        // running in Indexing or UI thread
        private void OnSearchingFinished(object sender, SearcherResult result)
        {
            try
            {
                if (result == null || result.SearchOptions == null || !result.SearchOptions.Equals(SearchOptions))
                {
                    // this means that the thread was cancelled or SearchOptions have been changed in the meantime, so we need to ignore the result.
                    return;
                }
                bool expandNewNodes = !String.IsNullOrWhiteSpace(SearchText);
                Application.Current.Dispatcher.Invoke(() =>
                {
					TreeViewModel.RefreshFromNode(result.ResultNode, result.Path, expandNewNodes);
					// when 'Sync with active document' is enabled and search results changed, we need to try to locate current document in the new search results
					ActiveDocumentPotentiallyChanged();
                });
            }
            finally
            {
                lock (this)
                {
					NumOfSearchingThreads--;
                }
            }
        }

        // running in Indexing thread
        private void OnIndexingProgress(object sender, string path)
        {
			RunSearch(path);
        }

        // running in UI thread
        private void OnIndexingFinished(object sender, IndexerResult result)
        {
            lock (this)
            {
				NumOfIndexingThreads--;
            }
        }

    }
}
