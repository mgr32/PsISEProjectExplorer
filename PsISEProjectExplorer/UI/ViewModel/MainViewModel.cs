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
                this.configHandler.SaveConfigValue("SearchRegex", value.ToString());
                this.commandExecutor.Execute<RecreateSearchTreeCommand>();
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
                this.commandExecutor.Execute<SyncWithActiveDocumentCommand>();
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

        private readonly ConfigHandler configHandler;

        public MainViewModel(ConfigHandler configHandler, WorkspaceDirectoryModel workspaceDirectoryModel,
            TreeViewModel treeViewModel, FilesPatternProvider filesPatternProvider, CommandExecutor commandExecutor)
        {
            this.configHandler = configHandler;
            this.WorkspaceDirectoryModel = workspaceDirectoryModel;
            this.TreeViewModel = treeViewModel;
            this.filesPatternProvider = filesPatternProvider;
            this.commandExecutor = commandExecutor;
            this.TreeViewModel.PropertyChanged += (s, e) => { if (e.PropertyName == "NumberOfFiles") this.OnPropertyChanged("TreeItemsResultString"); };

            this.searchRegex = configHandler.ReadConfigBoolValue("SearchRegex", false);
            this.searchInFiles = configHandler.ReadConfigBoolValue("SearchInFiles", true);
            this.showAllFiles = configHandler.ReadConfigBoolValue("ShowAllFiles", true);
            IEnumerable<string> excludePaths = configHandler.ReadConfigStringEnumerableValue("ExcludePaths");
            this.filesPatternProvider.IncludeAllFiles = this.showAllFiles;
            this.filesPatternProvider.ExcludePaths = excludePaths;
            
            this.syncWithActiveDocument = configHandler.ReadConfigBoolValue("SyncWithActiveDocument", false);
            var searchField = (this.searchInFiles ? FullTextFieldType.CatchAll : FullTextFieldType.Name);
            this.SearchOptions = new SearchOptions(searchField, string.Empty, this.searchRegex);          
        }      
        
    }
}
