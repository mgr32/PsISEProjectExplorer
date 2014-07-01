using NLog;
using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using PsISEProjectExplorer.Services;
using PsISEProjectExplorer.UI.Workers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace PsISEProjectExplorer.UI.ViewModel
{
    public class IndexingSearchingModel : BaseViewModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private BackgroundIndexer BackgroundIndexer { get; set; }

        private BackgroundSearcher BackgroundSearcher { get; set; }

        private EventHandler<IndexerResult> IndexerResultHandler { get; set; }

        private EventHandler<string> IndexerProgressHandler { get; set; }

        private EventHandler<SearcherResult> SearcherResultHandler { get; set; }

        public IndexingSearchingModel(EventHandler<SearcherResult> searcherResultHandler, EventHandler<IndexerResult> indexerResultHandler, EventHandler<string> indexerProgressHandler)
        {
            this.SearcherResultHandler = searcherResultHandler;
            this.IndexerResultHandler = indexerResultHandler;
            this.IndexerProgressHandler = indexerProgressHandler;
        }

        public void ReindexSearchTree(BackgroundIndexerParams indexerParams)
        {
            if (indexerParams.PathsChanged == null && this.BackgroundIndexer != null)
            {
                this.BackgroundIndexer.CancelAsync();
            }
            
            this.BackgroundIndexer = new BackgroundIndexer();
            this.BackgroundIndexer.RunWorkerCompleted += this.BackgroundIndexerWorkCompleted;
            this.BackgroundIndexer.ProgressChanged += this.BackgroundIndexerProgressChanged;
            this.BackgroundIndexer.RunWorkerAsync(indexerParams);
        }

        public void RunSearch(BackgroundSearcherParams searcherParams)
        {
            if (searcherParams.Path == null && this.BackgroundSearcher != null)
            {
                this.BackgroundSearcher.CancelAsync();
            }
            var searcher = new BackgroundSearcher();
            searcher.RunWorkerCompleted += this.BackgroundSearcherWorkCompleted;
            searcher.RunWorkerAsync(searcherParams);
            if (searcherParams.Path == null)
            {
                this.BackgroundSearcher = searcher;
            }
        }

        private void BackgroundIndexerWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                this.IndexerResultHandler(this, null);
                return;
            }
            var result = (IndexerResult)e.Result;
            if (result == null || !result.IsChanged)
            {
                this.IndexerResultHandler(this, null);
                return;
            }
            Logger.Debug("Indexing ended");
            this.IndexerResultHandler(this, result);
        }

        private void BackgroundIndexerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (this.IndexerProgressHandler != null)
            {
                Logger.Debug(String.Format("Indexer progress, path: {0}", (string)e.UserState));
                this.IndexerProgressHandler(this, (string)e.UserState);
            }
        }

        private void BackgroundSearcherWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                this.SearcherResultHandler(this, null);
                return;
            }
            var result = (SearcherResult)e.Result;
            if (result == null)
            {
                this.SearcherResultHandler(this, null);
                return;
            }
            Logger.Debug(String.Format("Searching ended, path: {0}", result.Path ?? "null"));
            this.SearcherResultHandler(this, result);
        }
    }
}
