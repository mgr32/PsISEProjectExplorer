using NLog;
using PsISEProjectExplorer.Services;
using PsISEProjectExplorer.UI.ViewModel;
using PsISEProjectExplorer.UI.Workers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PsISEProjectExplorer.Commands
{
    [Component]
    public class RunSearchCommand : ParameterizedCommand<string>
    {

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IList<BackgroundSearcher> backgroundSearchers;

        private readonly DocumentHierarchySearcher documentHierarchySearcher;

        private readonly MainViewModel mainViewModel;

        private readonly TreeViewModel treeViewModel;

        private readonly DocumentHierarchyFactory documentHierarchyFactory;

        private readonly ClearTreeViewCommand clearTreeViewCommand;

        private readonly SyncWithActiveDocumentCommand syncWithActiveDocumentCommand;

        public RunSearchCommand(DocumentHierarchySearcher documentHierarchySearcher, MainViewModel mainViewModel, TreeViewModel treeViewModel,
            DocumentHierarchyFactory documentHierarchyFactory, ClearTreeViewCommand clearTreeViewCommand, SyncWithActiveDocumentCommand syncWithActiveDocumentCommand)
        {
            this.documentHierarchySearcher = documentHierarchySearcher;
            this.mainViewModel = mainViewModel;
            this.treeViewModel = treeViewModel;
            this.documentHierarchyFactory = documentHierarchyFactory;
            this.clearTreeViewCommand = clearTreeViewCommand;
            this.syncWithActiveDocumentCommand = syncWithActiveDocumentCommand;
            this.backgroundSearchers = new List<BackgroundSearcher>();
        }
        // running in Indexing or UI thread
        public void Execute(string path)
        {
            this.mainViewModel.AddNumOfSearchingThreads(1);
            if (path == null)
            {
                this.clearTreeViewCommand.Execute();
            }
            var searcherParams = new BackgroundSearcherParams(this.documentHierarchyFactory.DocumentHierarchy, this.mainViewModel.SearchOptions, path);
            this.RunSearch(searcherParams);
        }


        // running in Indexing or UI thread
        private void RunSearch(BackgroundSearcherParams searcherParams)
        {
            if (searcherParams.Path == null)
            {
                lock (this.backgroundSearchers)
                {
                    foreach (var sear in this.backgroundSearchers)
                    {
                        sear.CancelAsync();
                    }
                    this.backgroundSearchers.Clear();
                }
            }
            var searcher = new BackgroundSearcher(this.documentHierarchySearcher);
            searcher.RunWorkerCompleted += this.BackgroundSearcherWorkCompleted;
            if (searcherParams.Path != null)
            {
                searcher.RunWorkerSync(searcherParams);
            }
            else
            {
                searcher.RunWorkerAsync(searcherParams);
                lock (this.backgroundSearchers)
                {
                    this.backgroundSearchers.Add(searcher);
                }
            }
        }

        // running in Indexing or UI thread
        private void BackgroundSearcherWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var searcher = sender as BackgroundSearcher;
            if (searcher != null)
            {
                lock (this.backgroundSearchers)
                {
                    this.backgroundSearchers.Remove(searcher);
                }
            }
            if (e.Cancelled)
            {
                return;
            }
            var result = (SearcherResult)e.Result;
            if (result == null)
            {
                return;
            }
            Logger.Debug(String.Format("Searching ended, path: {0}", result.Path ?? "null"));
            this.UpdateUI(result);
        }

        // running in Indexing or UI thread
        private void UpdateUI(SearcherResult result)
        {
            try
            {
                if (result == null || result.SearchOptions == null || !result.SearchOptions.Equals(this.mainViewModel.SearchOptions))
                {
                    // this means that the thread was cancelled or SearchOptions have been changed in the meantime, so we need to ignore the result.
                    return;
                }
                bool expandNewNodes = !String.IsNullOrWhiteSpace(this.mainViewModel.SearchText);
                if (Application.Current != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        this.treeViewModel.RefreshFromNode(result.ResultNode, result.Path, expandNewNodes);
                        // when 'Sync with active document' is enabled and search results changed, we need to try to locate current document in the new search results
                        this.syncWithActiveDocumentCommand.Execute(false);
                    });
                }
            }
            finally
            {
                this.mainViewModel.AddNumOfSearchingThreads(-1);
            }
        }
    }
}
