using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PsISEProjectExplorer.UI.Workers
{
    [Component]
    public class IndexingRunner
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IList<BackgroundIndexer> BackgroundIndexers { get; set; }

        private EventHandler<IndexerResult> IndexerResultHandler { get; set; }

        private EventHandler<string> IndexerProgressHandler { get; set; }

        public IndexingRunner()
        {
            this.BackgroundIndexers = new List<BackgroundIndexer>();
        }

        public void RegisterHandlers(EventHandler<IndexerResult> indexerResultHandler, EventHandler<string> indexerProgressHandler)
        {
            this.IndexerResultHandler = indexerResultHandler;
            this.IndexerProgressHandler = indexerProgressHandler;
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
    }
}
