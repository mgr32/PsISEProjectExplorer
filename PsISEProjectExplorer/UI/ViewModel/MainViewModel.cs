using PsISEProjectExplorer.DocHierarchy.HierarchyLogic;
using PsISEProjectExplorer.DocHierarchy.Nodes;
using PsISEProjectExplorer.EnumsAndOptions;
using PsISEProjectExplorer.IseIntegration;
using PsISEProjectExplorer.UI.Workers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        private DocumentHierarchies DocumentHierarchies { get; set; }

        private DocumentHierarchySearcher DocumentHierarchySearcher { get; set; }

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
                    this.RefreshSearchTree();
                }
            }

        }

        public MainViewModel()
        {
            this.TreeViewModel = new TreeViewModel();
            this.SearchOptions = new SearchOptions();
            this.SearchOptions.IncludeAllParents = true;
            this.SearchOptions.SearchField = FullTextFieldType.NAME;
            this.BackgroundIndexer = new BackgroundIndexer();
            this.BackgroundSearcher = new BackgroundSearcher();
            this.BackgroundIndexer.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.BackgroundIndexerWorkCompleted);
            this.BackgroundSearcher.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.BackgroundSearcherWorkCompleted);
            this.DocumentHierarchies = new DocumentHierarchies();
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
            INode node = (INode)e.Result;
            if (node == null)
            {
                this.TreeViewModel.RootTreeViewEntryItem = null;
                return;
            }
            bool expandNodes = !String.IsNullOrWhiteSpace(this.SearchText);
            var rootEntryItem = new TreeViewEntryItem(node);
            TreeViewEntryItem.MapToTreeViewEntryItem(node, rootEntryItem, expandNodes);
            this.TreeViewModel.RootTreeViewEntryItem = rootEntryItem;
        }

        private void RefreshSearchTree()
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
            BackgroundIndexerParams indexerParams = new BackgroundIndexerParams(this.DocumentHierarchies, this.rootDirectoryToSearch, null);
            this.IndexingInProgress = true;
            this.BackgroundIndexer.RunWorkerAsync(indexerParams);
        }

        private void RunSearch()
        {
            BackgroundSearcherParams searcherParams = new BackgroundSearcherParams(this.DocumentHierarchySearcher, this.SearchOptions, this.SearchText);
            this.SearchingInProgress = true;
            this.BackgroundSearcher.RunWorkerAsync(searcherParams);
        }

        private void OnFileTabChanged(object sender, IseEventArgs args)
        {
            this.RefreshSearchTree();
        }

        /*
       

        
       
        */
         
        public void GoToDefinition()
        {
            // TODO
        }

        public void FindAllReferences()
        {
            // TODO
        }

    }
}
