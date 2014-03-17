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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PsISEProjectExplorer.UI.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
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
                if (!String.IsNullOrEmpty(this.SearchText))
                {
                    this.SearchOptions.SearchField = (this.searchInFiles ? FullTextFieldType.CATCH_ALL : FullTextFieldType.NAME);
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
            this.BackgroundIndexer = new BackgroundIndexer();
            this.BackgroundSearcher = new BackgroundSearcher();
            this.BackgroundIndexer.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.BackgroundIndexerWorkCompleted);
            this.BackgroundSearcher.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.BackgroundSearcherWorkCompleted);
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
            PowershellFunctionNode node = (PowershellFunctionNode)this.DocumentHierarchySearcher.GetFunctionNodeByName(funcName);
            if (node == null)
            {
                return;
            }
            this.IseIntegrator.GoToFile(node.FilePath);
            this.IseIntegrator.SetCursor(node.PowershellFunction.StartLine, node.PowershellFunction.StartColumn);
            
        }

        public void FindAllReferences()
        {
            string funcName = this.GetFunctionNameAtCurrentPosition();
            if (funcName == null)
            {
                return;
            }
            this.searchText = funcName;
            this.SearchInFiles = true;
            this.OnPropertyChanged("SearchText");
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
            this.DocumentHierarchySearcher = (DocumentHierarchySearcher)e.Result;
            if (!this.SearchTreeInitialized)
            {
                this.RunSearch();
                this.SearchTreeInitialized = true;
            }

        }

        private void BackgroundSearcherWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.SearchingInProgress = false;
            INode rootNode = (INode)e.Result;
            this.TreeViewModel.TreeViewOptions.ExpandAllNodes = !String.IsNullOrWhiteSpace(this.SearchText);
            this.TreeViewModel.RefreshFromRoot(rootNode);
        }

        private void OnFileSystemChanged(object sender, EventArgs args)
        {
            // TODO: reindex incrementally
            Application.Current.Dispatcher.Invoke(new Action(() => { this.ReindexSearchTree(); }));
        }

        private void ReloadRootDirectory()
        {
            var selectedFilePath = this.IseIntegrator.SelectedFilePath;
            string newRootDirectoryToSearch = RootDirectoryProvider.GetRootDirectoryToSearch(selectedFilePath);
            if (newRootDirectoryToSearch != null && (this.RootDirectoryToSearch == null || !newRootDirectoryToSearch.StartsWith(this.RootDirectoryToSearch)))
            {
                this.RootDirectoryToSearch = newRootDirectoryToSearch;
                this.ReindexSearchTree();
            }

        }

        private void ReindexSearchTree()
        {
            this.SearchTreeInitialized = false;
            if (this.IndexingInProgress)
            {
                // TODO: handle it more nicely
                return;
            }
            BackgroundIndexerParams indexerParams = new BackgroundIndexerParams(this.DocumentHierarchyIndexer, this.rootDirectoryToSearch, null);
            this.IndexingInProgress = true;
            this.BackgroundIndexer.RunWorkerAsync(indexerParams);
        }

        private void RunSearch()
        {
            BackgroundSearcherParams searcherParams = new BackgroundSearcherParams(this.DocumentHierarchySearcher, this.SearchOptions, this.SearchText);
            if (this.SearchingInProgress)
            {
                // TODO: handle it more nicely
                return;
            }
            this.SearchingInProgress = true;           
            this.BackgroundSearcher.RunWorkerAsync(searcherParams);
        }

        private void OnFileTabChanged(object sender, IseEventArgs args)
        {
            this.ReloadRootDirectory();
        }      

    }
}
