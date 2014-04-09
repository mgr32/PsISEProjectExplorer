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
using System.IO;
using System.Linq;
using System.Windows;

namespace PsISEProjectExplorer.UI.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public TreeViewModel TreeViewModel { get; private set; }

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
                    this.RecalculateRootDirectory(false);
                }
                ConfigHandler.SaveConfigValue("AutoUpdateRootDirectory", value.ToString());
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
                this.RecalculateRootDirectory(true);
            }

        }

        public MainViewModel()
        {
            this.autoUpdateRootDirectory = ConfigHandler.ReadConfigBoolValue("AutoUpdateRootDirectory", true);
            this.searchInFiles = ConfigHandler.ReadConfigBoolValue("SearchInFiles", false);
            var searchField = (this.searchInFiles ? FullTextFieldType.CatchAll : FullTextFieldType.Name);
            this.rootDirectoryToSearch = ConfigHandler.ReadConfigStringValue("RootDirectory");
            if (this.rootDirectoryToSearch == string.Empty || !Directory.Exists(this.rootDirectoryToSearch))
            { 
                this.rootDirectoryToSearch = null;
            }
            this.TreeViewModel = new TreeViewModel();
            this.SearchOptions = new SearchOptions { IncludeAllParents = true, SearchField = searchField };
            this.DocumentHierarchyIndexer = new DocumentHierarchyFactory();
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
            this.TreeViewModel.RefreshFromRoot(rootNode, expandNewNodes);
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
            this.AutoUpdateRootDirectory = false;
            this.RootDirectoryToSearch = newPath;
            this.RecalculateRootDirectory(false);
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
            var indexerParams = new BackgroundIndexerParams(this.DocumentHierarchyIndexer, this.rootDirectoryToSearch, pathsChanged);
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
            if (this.AutoUpdateRootDirectory)
            {
                this.RecalculateRootDirectory(false);
            }
        }      

    }
}
