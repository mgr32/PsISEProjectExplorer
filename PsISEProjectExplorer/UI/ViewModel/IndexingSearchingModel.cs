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
			SearcherResultHandler = searcherResultHandler;
			IndexerResultHandler = indexerResultHandler;
			IndexerProgressHandler = indexerProgressHandler;
			BackgroundIndexers = new List<BackgroundIndexer>();
			BackgroundSearchers = new List<BackgroundSearcher>();
        }

        // running in UI thread
        public void ReindexSearchTree(BackgroundIndexerParams indexerParams)
        {
            if (indexerParams.PathsChanged == null)
            {
                lock (BackgroundIndexers)
                {
                    foreach (var ind in BackgroundIndexers)
                    {
                        ind.CancelAsync();
                    }
					BackgroundIndexers.Clear();
                }
            }

            var indexer = new BackgroundIndexer();
            indexer.RunWorkerCompleted += BackgroundIndexerWorkCompleted;
            indexer.ProgressChanged += BackgroundIndexerProgressChanged;
            indexer.RunWorkerAsync(indexerParams);
            lock (BackgroundIndexers)
            {
				BackgroundIndexers.Add(indexer);
            }
        }

        // running in Indexing or UI thread
        public void RunSearch(BackgroundSearcherParams searcherParams)
        {
            if (searcherParams.Path == null)
            {
                lock (BackgroundSearchers)
                {
                    foreach (var sear in BackgroundSearchers)
                    {
                        sear.CancelAsync();
                    }
					BackgroundSearchers.Clear();
                }
            }
            var searcher = new BackgroundSearcher();
            searcher.RunWorkerCompleted += BackgroundSearcherWorkCompleted;
            if (searcherParams.Path != null)
            {
                searcher.RunWorkerSync(searcherParams);
            }
            else
            {
                searcher.RunWorkerAsync(searcherParams);
                lock (BackgroundSearchers)
                {
					BackgroundSearchers.Add(searcher);
                }
            }
        }

        // running in UI thread
        private void BackgroundIndexerWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var indexer = sender as BackgroundIndexer;
            if (indexer != null)
            {
                lock (BackgroundIndexers)
                {
					BackgroundIndexers.Remove(indexer);
                }
            }
            if (e.Cancelled)
            {
				IndexerResultHandler(this, null);
                return;
            }
            var result = (IndexerResult)e.Result;
            if (result == null || !result.IsChanged)
            {
				IndexerResultHandler(this, null);
                return;
            }
            Logger.Debug("Indexing ended");
			IndexerResultHandler(this, result);
        }

        // running in Indexing thread
        private void BackgroundIndexerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (IndexerProgressHandler != null)
            {
                Logger.Debug(String.Format("Indexer progress, path: {0}", (string)e.UserState));
				IndexerProgressHandler(this, (string)e.UserState);
            }
        }

        // running in Indexing or UI thread
        private void BackgroundSearcherWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var searcher = sender as BackgroundSearcher;
            if (searcher != null)
            {
                lock (BackgroundSearchers)
                {
					BackgroundSearchers.Remove(searcher);
                }
            }
            if (e.Cancelled)
            {
				SearcherResultHandler(this, null);
                return;
            }
            var result = (SearcherResult)e.Result;
            if (result == null)
            {
				SearcherResultHandler(this, null);
                return;
            }
            Logger.Debug(String.Format("Searching ended, path: {0}", result.Path ?? "null"));
			SearcherResultHandler(this, result);
        }
    }
}
