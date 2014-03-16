using Microsoft.PowerShell.Host.ISE;
using ProjectExplorer.DocHierarchy;
using ProjectExplorer.DocHierarchy.FullText;
using ProjectExplorer.DocHierarchy.HierarchyLogic;
using ProjectExplorer.DocHierarchy.Nodes;
using ProjectExplorer.EnumsAndOptions;
using ProjectExplorer.TreeView;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace ProjectExplorer
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ProjectExplorerWindow : UserControl, IAddOnToolHostObject, INotifyPropertyChanged
    {

        private readonly BackgroundIndexer backgroundIndexer = new BackgroundIndexer();

        private readonly BackgroundSearcher backgroundSearcher = new BackgroundSearcher();

        private DocumentHierarchies DocumentHierarchies = new DocumentHierarchies();

        private DocumentHierarchySearcher DocumentHierarchySearcher;

        private TreeViewEntryItem rootTreeViewEntryItem;

        public TreeViewEntryItem RootTreeViewEntryItem
        {
            get { return this.rootTreeViewEntryItem; }
            set
            {
                this.rootTreeViewEntryItem = value;
                this.PropertyChanged(this, new PropertyChangedEventArgs("TreeViewItems"));
            }
        }

        public IList<TreeViewEntryItem> TreeViewItems
        {
            get
            {
                if (this.RootTreeViewEntryItem == null)
                {
                    return new List<TreeViewEntryItem>();
                }
                return this.RootTreeViewEntryItem.Children;
            }
        }

        private void OnIseTabChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LastEditorWithFocus")
            {
                RefreshSearchTree();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName]string propName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        private ObjectModelRoot hostObject;

         // Entry point to the ISE object model.
        public ObjectModelRoot HostObject
        {
            get { return hostObject; }
            set { this.hostObject = value; OnHostObjectSet(); }
        }

        private string rootDirectoryToSearch;

        public string RootDirectoryToSearch
        {
            get { return this.rootDirectoryToSearch; }
            set
            {
                this.rootDirectoryToSearch = value;
                this.DocumentHierarchySearcher = null;
                PropertyChanged(this, new PropertyChangedEventArgs("RootDirectoryToSearch"));
                PropertyChanged(this, new PropertyChangedEventArgs("RootDirectoryLabel"));
            }
        }

        private bool indexingInProgress;

        public bool IndexingInProgress
        {
            get { return this.indexingInProgress; }
            set
            {
                this.indexingInProgress = value;
                PropertyChanged(this, new PropertyChangedEventArgs("IndexingInProgress"));

            }
        }

        private bool searchingInProgress;

        public bool SearchingInProgress
        {
            get { return this.searchingInProgress; }
            set
            {
                this.searchingInProgress = value;
                PropertyChanged(this, new PropertyChangedEventArgs("SearchingInProgress"));

            }
        }

        private string searchText;

        public string SearchText
        {
            get { return this.searchText; }
            set
            {
                this.searchText = value;
                PropertyChanged(this, new PropertyChangedEventArgs("SearchText"));
                this.RunSearch();
            }
        }

        private bool searchTreeInitialized;

        public string RootDirectoryLabel
        {
            get { return "Search in directory " + RootDirectoryToSearch; }
        }

        private SearchOptions searchOptions = new SearchOptions();


        private void OnHostObjectSet()
        {
            HostObject.CurrentPowerShellTab.PropertyChanged += OnIseTabChanged;
            if (HostObject.CurrentPowerShellTab.Files.SelectedFile != null)
            {
                RefreshSearchTree();
            }
        }

        public ProjectExplorerWindow()
        {
            this.DataContext = this;
            this.searchOptions.IncludeAllParents = true;
            this.searchOptions.SearchField = FullTextFieldType.CATCH_ALL;
            this.backgroundIndexer.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.BackgroundIndexerWorkCompleted);
            this.backgroundSearcher.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.BackgroundSearcherWorkCompleted);
            InitializeComponent();
            
        }

        private void RefreshSearchTree()
        {
            string oldRootDirectoryToSearch = this.rootDirectoryToSearch;
            this.RootDirectoryToSearch = RootDirectoryProvider.GetRootDirectoryToSearch(HostObject.CurrentPowerShellTab.Files.SelectedFile.FullPath);
            if (oldRootDirectoryToSearch != this.RootDirectoryToSearch)
            {
                this.ReindexSearchTree();
            }
                       
        }

        private void ReindexSearchTree()
        {
            BackgroundIndexerParams indexerParams = new BackgroundIndexerParams(this.DocumentHierarchies, this.rootDirectoryToSearch, null);
            this.IndexingInProgress = true;
            this.backgroundIndexer.RunWorkerAsync(indexerParams);
        }

        private void BackgroundIndexerWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.IndexingInProgress = false;
            this.DocumentHierarchySearcher = (DocumentHierarchySearcher)e.Result;
            if (!this.searchTreeInitialized)
            {
                this.RunSearch();
                this.searchTreeInitialized = true;
            }
            
        }

        private void RunSearch()
        {
            BackgroundSearcherParams searcherParams = new BackgroundSearcherParams(this.DocumentHierarchySearcher, this.searchOptions, this.SearchText);
            this.SearchingInProgress = true;
            this.backgroundSearcher.RunWorkerAsync(searcherParams);
        }

        private void BackgroundSearcherWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.SearchingInProgress = false;
            INode node = (INode)e.Result;
            if (node == null)
            {
                this.RootTreeViewEntryItem = null;
                return;
            }
            bool expandNodes = !String.IsNullOrWhiteSpace(this.SearchText);
            var rootEntryItem = new TreeViewEntryItem(node);
            this.MapToTreeViewEntryItem(node, rootEntryItem, expandNodes);
            this.RootTreeViewEntryItem = rootEntryItem;
           
        }

        private void MapToTreeViewEntryItem(INode node, TreeViewEntryItem treeViewEntryItem, bool expandNodes)
        {
            foreach (INode child in node.Children)
            {
                TreeViewEntryItem newItem = new TreeViewEntryItem(child);
                newItem.IsExpanded = expandNodes;
                treeViewEntryItem.Children.Add(newItem);
                this.MapToTreeViewEntryItem(child, newItem, expandNodes);
            }
        }

        private void SearchResults_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SelectItem();
        }

        private void SearchResults_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SelectItem();
            }
        }

        private void SelectItem()
        {
            TreeViewEntryItem selectedItem = (TreeViewEntryItem)this.SearchResults.SelectedItem;
            if (selectedItem != null)
            {
                if (selectedItem.Node.NodeType == NodeType.FILE)
                {
                    HostObject.CurrentPowerShellTab.Files.Add(selectedItem.Node.Path);
                }
                else if (selectedItem.Node.NodeType == NodeType.FUNCTION)
                {
                    PowershellFunctionNode node = ((PowershellFunctionNode)selectedItem.Node);
                    HostObject.CurrentPowerShellTab.Files.Add(node.FilePath);
                    HostObject.CurrentPowerShellTab.Files.SelectedFile.Editor.SetCaretPosition(node.PowershellFunction.StartLine, node.PowershellFunction.StartColumn);
                }
            }
        }
    }
}
