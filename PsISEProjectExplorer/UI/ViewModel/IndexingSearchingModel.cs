using NLog;
using PsISEProjectExplorer.UI.Workers;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PsISEProjectExplorer.UI.ViewModel
{
    public class IndexingSearchingModel : BaseViewModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IList<BackgroundIndexer> BackgroundIndexers { get; set; }

        private IList<BackgroundSearcher> BackgroundSearchers { get; set; }

        private EventHandler<IndexerResult> IndexerResultHandler { get; set; }

        private EventHandler<string> IndexerProgressHandler { get; set; }

        private EventHandler<SearcherResult> SearcherResultHandler { get; set; }

        public IndexingSearchingModel(EventHandler<SearcherResult> searcherResultHandler, EventHandler<IndexerResult> indexerResultHandler, EventHandler<string> indexerProgressHandler)
        {
            this.SearcherResultHandler = searcherResultHandler;
            this.IndexerResultHandler = indexerResultHandler;
            this.IndexerProgressHandler = indexerProgressHandler;
            this.BackgroundIndexers = new List<BackgroundIndexer>();
            this.BackgroundSearchers = new List<BackgroundSearcher>();
        }

        // running in UI thread
        public void ReindexSearchTree(BackgroundIndexerParams indexerParams)
        {
            if (indexerParams.PathsChanged == null)
            {
                lock (this.BackgroundIndexers)
                {
                    foreach (var ind in this.BackgroundIndexers)
                    {
                        ind.CancelAsync();
                    }
                    this.BackgroundIndexers.Clear();
                }
            }

            var indexer = new BackgroundIndexer();
            indexer.RunWorkerCompleted += this.BackgroundIndexerWorkCompleted;
            indexer.ProgressChanged += this.BackgroundIndexerProgressChanged;
            indexer.RunWorkerAsync(indexerParams);
            lock (this.BackgroundIndexers)
            {
                this.BackgroundIndexers.Add(indexer);
            }
        }

        // running in Indexing or UI thread
        public void RunSearch(BackgroundSearcherParams searcherParams)
        {
            if (searcherParams.Path == null)
            {
                lock (this.BackgroundSearchers)
                {
                    foreach (var sear in this.BackgroundSearchers)
                    {
                        sear.CancelAsync();
                    }
                    this.BackgroundSearchers.Clear();
                }
            }
            var searcher = new BackgroundSearcher();
            searcher.RunWorkerCompleted += this.BackgroundSearcherWorkCompleted;
            if (searcherParams.Path != null)
            {
                searcher.RunWorkerSync(searcherParams);
            }
            else
            {
                searcher.RunWorkerAsync(searcherParams);
                lock (this.BackgroundSearchers)
                {
                    this.BackgroundSearchers.Add(searcher);
                }
            }
        }

        // running in UI thread
        private void BackgroundIndexerWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var indexer = sender as BackgroundIndexer;
            if (indexer != null)
            {
                lock (this.BackgroundIndexers)
                {
                    this.BackgroundIndexers.Remove(indexer);
                }
            }
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

        // running in Indexing thread
        private void BackgroundIndexerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (this.IndexerProgressHandler != null)
            {
                Logger.Debug(String.Format("Indexer progress, path: {0}", (string)e.UserState));
                this.IndexerProgressHandler(this, (string)e.UserState);
            }
        }

        // running in Indexing or UI thread
        private void BackgroundSearcherWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var searcher = sender as BackgroundSearcher;
            if (searcher != null)
            {
                lock (this.BackgroundSearchers)
                {
                    this.BackgroundSearchers.Remove(searcher);
                }
            }
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
