using NLog;
using PsISEProjectExplorer.EnumsAndOptions;
using PsISEProjectExplorer.Model;
using PsISEProjectExplorer.Model.DocHierarchy;
using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using PsISEProjectExplorer.Services;
using PsISEProjectExplorer.UI.IseIntegration;
using PsISEProjectExplorer.UI.Workers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PsISEProjectExplorer.UI.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public TreeViewModel TreeViewModel { get; private set; }

        private string rootDirectoryToSearch;

        public string RootDirectoryToSearch
        {
            get { return this.rootDirectoryToSearch; }
            set
            {
                this.rootDirectoryToSearch = value;
                this.DocumentHierarchySearcher = null;
                this.OnPropertyChanged();
                this.OnPropertyChanged("RootDirectoryLabel");
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

        private bool searchingInProgress;

        public bool SearchingInProgress
        {
            get { return this.searchingInProgress; }
            set
            {
                this.searchingInProgress = value;
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
                logger.Debug("Search text changed to: " + this.searchText);
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
                this.SearchOptions.SearchField = (this.searchInFiles ? FullTextFieldType.CATCH_ALL : FullTextFieldType.NAME);
                if (!String.IsNullOrEmpty(this.SearchText))
                {
                    this.RunSearch();
                }
            }
        }

        public string RootDirectoryLabel
        {
            get { return "Project root: " + this.RootDirectoryToSearch; }
        }
       
        private bool SearchTreeInitialized { get; set; }

        private SearchOptions SearchOptions { get; set; }

        private BackgroundIndexer BackgroundIndexer { get; set; }

        private BackgroundSearcher BackgroundSearcher { get; set; }

        private DocumentHierarchySearcher DocumentHierarchySearcher { get; set; }

        private DocumentHierarchyFactory DocumentHierarchyIndexer { get; set; }

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
                if (this.IseIntegrator.SelectedFilePath != null)
                {
                    this.ReloadRootDirectory();
                }
            }

        }

        public MainViewModel()
        {
            this.TreeViewModel = new TreeViewModel();
            this.SearchOptions = new SearchOptions { IncludeAllParents = true, SearchField = FullTextFieldType.NAME };
            this.DocumentHierarchyIndexer = new DocumentHierarchyFactory();
            FileSystemChangeNotifier.FileSystemChanged += OnFileSystemChanged;
            FileSystemChangeNotifier.FileSystemRenamed += OnFileSystemRenamed;
        }

        public void GoToDefinition()
        {
            string funcName = this.GetFunctionNameAtCurrentPosition();
            if (funcName == null)
            {
                return;
            }
            PowershellFunctionNode node = (PowershellFunctionNode)this.DocumentHierarchySearcher.GetFunctionNodeByName(funcName);
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
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                // TODO: this is hacky...
                this.searchText = string.Empty;
                this.SearchInFiles = true;
                this.SearchText = funcName;
               
            }));
            
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
            WorkerResult result = (WorkerResult)e.Result;
            if (result.StartTimestamp != this.LastIndexStartTime)
            {
                return;
            }
            logger.Debug("Indexing ended, searchTreeInitialized: " + this.SearchTreeInitialized);
            this.DocumentHierarchySearcher = (DocumentHierarchySearcher)result.Result;
            if (!this.SearchTreeInitialized)
            {
                this.RunSearch();
                this.SearchTreeInitialized = true;
            }

        }

        private void BackgroundSearcherWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.SearchingInProgress = false;
            WorkerResult result = (WorkerResult)e.Result;
            if (result.StartTimestamp != this.LastSearchStartTime)
            {
                return;
            }
            logger.Debug("Searching ended");
            INode rootNode = (INode)result.Result;
            bool expandNewNodes = !String.IsNullOrWhiteSpace(this.SearchText);
            this.TreeViewModel.RefreshFromRoot(rootNode, expandNewNodes);
        }

        private void OnFileSystemChanged(object sender, FileSystemEventArgs args)
        {
            IList<string> pathsChanged = new List<string>() { args.FullPath };
            // workspace directory change
            if (args.FullPath.ToLowerInvariant() == this.rootDirectoryToSearch.ToLowerInvariant())
            {
                pathsChanged = null;
            }
            logger.Debug("OnFileSystemChanged: " + string.Join(",", pathsChanged));
            Application.Current.Dispatcher.Invoke(new Action(() => { this.ReindexSearchTree(pathsChanged); }));
        }

        private void OnFileSystemRenamed(object sender, RenamedEventArgs args)
        {
            IList<string> pathsChanged = new List<string>();
            // workspace directory change
            if (args.OldFullPath.ToLowerInvariant() == this.rootDirectoryToSearch.ToLowerInvariant())
            {
                pathsChanged = null;
            }
            else
            {
                pathsChanged.Add(args.OldFullPath);
                if (args.OldFullPath.ToLowerInvariant() != args.FullPath.ToLowerInvariant())
                {
                    pathsChanged.Add(args.FullPath);
                }
            }
            logger.Debug("OnFileSystemRenamed: " + string.Join(",", pathsChanged));
            Application.Current.Dispatcher.Invoke(new Action(() => { this.ReindexSearchTree(pathsChanged); }));
        }

        private void ReloadRootDirectory()
        {
            var selectedFilePath = this.IseIntegrator.SelectedFilePath;
            string newRootDirectoryToSearch = RootDirectoryProvider.GetRootDirectoryToSearch(selectedFilePath);
            if (newRootDirectoryToSearch != null && (this.RootDirectoryToSearch == null || !newRootDirectoryToSearch.StartsWith(this.RootDirectoryToSearch)))
            {
                this.RootDirectoryToSearch = newRootDirectoryToSearch;
                this.ReindexSearchTree(null);
            }

        }

        private void ReindexSearchTree(IEnumerable<string> pathsChanged)
        {
            this.SearchTreeInitialized = false;
            BackgroundIndexerParams indexerParams = new BackgroundIndexerParams(this.DocumentHierarchyIndexer, this.rootDirectoryToSearch, pathsChanged);
            this.IndexingInProgress = true;
            this.BackgroundIndexer = new BackgroundIndexer();
            this.LastIndexStartTime = this.BackgroundIndexer.StartTimestamp;
            this.BackgroundIndexer.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.BackgroundIndexerWorkCompleted);
            this.BackgroundIndexer.RunWorkerAsync(indexerParams);
        }

        private void RunSearch()
        {
            BackgroundSearcherParams searcherParams = new BackgroundSearcherParams(this.DocumentHierarchySearcher, this.SearchOptions, this.SearchText);
            this.SearchingInProgress = true;
            this.BackgroundSearcher = new BackgroundSearcher();
            this.LastSearchStartTime = this.BackgroundSearcher.StartTimestamp;
            this.BackgroundSearcher.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.BackgroundSearcherWorkCompleted);
            this.BackgroundSearcher.RunWorkerAsync(searcherParams);
        }

        private void OnFileTabChanged(object sender, IseEventArgs args)
        {
            this.ReloadRootDirectory();
        }      

    }
}
