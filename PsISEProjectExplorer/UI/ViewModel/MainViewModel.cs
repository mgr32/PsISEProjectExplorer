using NLog;
using PsISEProjectExplorer.Commands;
using PsISEProjectExplorer.Config;
using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.Model;
using PsISEProjectExplorer.Services;
using System;
using System.Collections.Generic;

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
                this.configValues.SearchRegex = value;
                this.commandExecutor.ExecuteWithParam<ReindexSearchTreeCommand, IEnumerable<string>>(null);
            }
        }

        private IndexingMode indexFilesMode;

        public IndexingMode IndexFilesMode
        {
            get { return this.indexFilesMode; }
            set
            {
                this.indexFilesMode = value;
                this.filesPatternProvider.IndexFilesMode = value;
                this.OnPropertyChanged();
                this.SearchOptions.SearchField = (indexFilesMode != IndexingMode.NO_FILES ? FullTextFieldType.CatchAll : FullTextFieldType.Name);
                this.configValues.IndexFilesMode = indexFilesMode;
                this.commandExecutor.ExecuteWithParam<ReindexSearchTreeCommand, IEnumerable<string>>(null);
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
                this.configValues.ShowAllFiles = value;
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
                this.commandExecutor.ExecuteWithParam<SyncWithActiveDocumentCommand, bool>(true);
                this.configValues.SyncWithActiveDocument = value;
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

        public bool AnalyzeDocumentContents
        {
            get
            {
                return !this.SearchRegex;
            }
        }

        public SearchOptions SearchOptions { get; private set; }

        private readonly FilesPatternProvider filesPatternProvider;

        private readonly CommandExecutor commandExecutor;

        private readonly ConfigValues configValues;

        public MainViewModel(ConfigValues configValues, WorkspaceDirectoryModel workspaceDirectoryModel,
            TreeViewModel treeViewModel, FilesPatternProvider filesPatternProvider, CommandExecutor commandExecutor)
        {
            this.configValues = configValues;
            this.WorkspaceDirectoryModel = workspaceDirectoryModel;
            this.TreeViewModel = treeViewModel;
            this.filesPatternProvider = filesPatternProvider;
            this.commandExecutor = commandExecutor;
            this.TreeViewModel.PropertyChanged += (s, e) => { if (e.PropertyName == "NumberOfFiles") this.OnPropertyChanged("TreeItemsResultString"); };

            this.searchRegex = configValues.SearchRegex;
            this.indexFilesMode = configValues.IndexFilesMode;
            this.showAllFiles = configValues.ShowAllFiles;
            this.filesPatternProvider.IncludeAllFiles = this.showAllFiles;
            this.filesPatternProvider.ExcludePaths = configValues.ExcludePaths;
            this.filesPatternProvider.IndexFilesMode = configValues.IndexFilesMode;

            this.syncWithActiveDocument = configValues.SyncWithActiveDocument;
            var searchField = (indexFilesMode != IndexingMode.NO_FILES ? FullTextFieldType.CatchAll : FullTextFieldType.Name);
            this.SearchOptions = new SearchOptions(searchField, string.Empty, this.searchRegex);          
        }      
        
    }
}
