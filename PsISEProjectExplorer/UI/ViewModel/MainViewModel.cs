using NLog;
using PsISEProjectExplorer.Commands;
using PsISEProjectExplorer.Config;
using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.Model;
using PsISEProjectExplorer.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace PsISEProjectExplorer.UI.ViewModel
{
    [Component]
    public class MainViewModel : BaseViewModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public TreeViewModel TreeViewModel { get; private set; }

        public WorkspaceDirectoryModel WorkspaceDirectoryModel { get; private set; }

        public string SearchText
        {
            get { return this.SearchOptions.SearchText; }
            set
            {
                this.SearchOptions.SearchText = value;
                Logger.Debug("Search text changed to: " + this.SearchOptions.SearchText);
                this.OnPropertyChanged();
                this.commandExecutor.ExecuteWithParam<RunSearchCommand, string>(null);
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
                this.documentHierarchyFactory.CreateDocumentHierarchy(this.WorkspaceDirectoryModel.CurrentWorkspaceDirectory, this.AnalyzeDocumentContents);
                this.commandExecutor.ExecuteWithParam<ReindexSearchTreeCommand, IEnumerable<string>>(null);
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
                    this.commandExecutor.ExecuteWithParam<RunSearchCommand, string>(null);
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
                this.filesPatternProvider.IncludeAllFiles = value;
                this.OnPropertyChanged();
                this.configHandler.SaveConfigValue("ShowAllFiles", value.ToString());
                this.commandExecutor.ExecuteWithParam<ReindexSearchTreeCommand, IEnumerable<string>>(null);
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

        private int NumOfSearchingThreads { get; set; }

        private int NumOfIndexingThreads { get; set; }

        public void AddNumOfSearchingThreads(int value)
        {
            lock (this)
            {
                this.NumOfSearchingThreads = this.NumOfSearchingThreads + value;
                this.OnPropertyChanged();
                this.OnPropertyChanged("TreeItemsResultString");
            }
        }

        public void AddNumOfIndexingThreads(int value)
        {
            lock (this)
            {
                this.NumOfIndexingThreads = this.NumOfIndexingThreads + value;
                this.OnPropertyChanged();
                this.OnPropertyChanged("TreeItemsResultString");
            }
        }


        public SearchOptions SearchOptions { get; private set; }

        private readonly DocumentHierarchyFactory documentHierarchyFactory;

        private readonly FilesPatternProvider filesPatternProvider;

        private readonly FileSystemChangeWatcher fileSystemChangeWatcher;

        private readonly CommandExecutor commandExecutor;

        private bool AnalyzeDocumentContents
        {
            get
            {
                return !this.SearchRegex;
            }
        }

        private readonly ConfigHandler configHandler;

        public MainViewModel(ConfigHandler configHandler, WorkspaceDirectoryModel workspaceDirectoryModel, DocumentHierarchyFactory documentHierarchyFactory,
            FileSystemChangeWatcher fileSystemChangeWatcher, TreeViewModel treeViewModel, FilesPatternProvider filesPatternProvider, CommandExecutor commandExecutor)
        {
            this.configHandler = configHandler;
            this.WorkspaceDirectoryModel = workspaceDirectoryModel;
            this.documentHierarchyFactory = documentHierarchyFactory;
            this.fileSystemChangeWatcher = fileSystemChangeWatcher;
            this.TreeViewModel = treeViewModel;
            this.filesPatternProvider = filesPatternProvider;
            this.commandExecutor = commandExecutor;
            this.TreeViewModel.PropertyChanged += (s, e) => { if (e.PropertyName == "NumberOfFiles") this.OnPropertyChanged("TreeItemsResultString"); };

            fileSystemChangeWatcher.RegisterOnChangeCallback(this.ReindexOnFileSystemChanged);

            this.searchRegex = configHandler.ReadConfigBoolValue("SearchRegex", false);
            this.searchInFiles = configHandler.ReadConfigBoolValue("SearchInFiles", true);
            this.showAllFiles = configHandler.ReadConfigBoolValue("ShowAllFiles", true);
            IEnumerable<string> excludePaths = configHandler.ReadConfigStringEnumerableValue("ExcludePaths");
            this.filesPatternProvider.IncludeAllFiles = this.showAllFiles;
            this.filesPatternProvider.ExcludePaths = excludePaths;
            
            this.syncWithActiveDocument = configHandler.ReadConfigBoolValue("SyncWithActiveDocument", false);
            var searchField = (this.searchInFiles ? FullTextFieldType.CatchAll : FullTextFieldType.Name);
            this.SearchOptions = new SearchOptions(searchField, string.Empty, this.searchRegex);

            if (this.WorkspaceDirectoryModel.CurrentWorkspaceDirectory != null)
            {
                this.documentHierarchyFactory.CreateDocumentHierarchy(this.WorkspaceDirectoryModel.CurrentWorkspaceDirectory, this.AnalyzeDocumentContents);
            }
            this.WorkspaceDirectoryModel.PropertyChanged += this.OnWorkspaceDirectoryChanged;

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
            this.commandExecutor.ExecuteWithParam<ReindexSearchTreeCommand, IEnumerable<string>>(pathsChanged);
        }

        // TODO: get it out of here
        public void ActiveDocumentPotentiallyChanged()
        {
            if (this.SyncWithActiveDocument)
            {
                // TODO: this should be suppressed during indexing
                this.commandExecutor.Execute<LocateFileInTreeCommand>();
            }
        }

        private void OnWorkspaceDirectoryChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentWorkspaceDirectory")
            {
                this.documentHierarchyFactory.CreateDocumentHierarchy(this.WorkspaceDirectoryModel.CurrentWorkspaceDirectory, this.AnalyzeDocumentContents);
                this.commandExecutor.ExecuteWithParam<ReindexSearchTreeCommand, IEnumerable<string>>(null);
            }
        }
        
    }
}
