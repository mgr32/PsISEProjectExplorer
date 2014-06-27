using NLog;
using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using PsISEProjectExplorer.Services;
using PsISEProjectExplorer.UI.Workers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.UI.ViewModel
{
    public class IndexingSearchingModel : BaseViewModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private int numOfIndexingThreads;

        private int NumOfIndexingThreads
        {
            get
            {
                return this.numOfIndexingThreads;
            }
            set
            {
                this.numOfIndexingThreads = value;
                this.OnPropertyChanged("IndexingInProgress");
            }
        }

        public bool IndexingInProgress
        {
            get
            {
                lock (this.indexingLock)
                {
                    return this.NumOfIndexingThreads > 0;
                }

            }
        }


        private BackgroundIndexer BackgroundIndexer { get; set; }

        private BackgroundSearcher BackgroundSearcher { get; set; }

        private object indexingLock = new object();

        private object searchingLock = new object();

        private DateTime LastIndexStartTime { get; set; }

        private DateTime LastSearchStartTime { get; set; }

        private EventHandler<IndexerResult> IndexerResultHandler { get; set; }

        private EventHandler<SearcherResult> SearcherResultHandler { get; set; }

        public IndexingSearchingModel(EventHandler<SearcherResult> searcherResultHandler, EventHandler<IndexerResult> indexerResultHandler)
        {
            this.SearcherResultHandler = searcherResultHandler;
            this.IndexerResultHandler = indexerResultHandler;
        }

        public void ReindexSearchTree(BackgroundIndexerParams indexerParams)
        {
            lock (this.indexingLock)
            {
                if (this.BackgroundIndexer != null)
                {
                    this.BackgroundIndexer.CancelAsync();
                }
                this.NumOfIndexingThreads++;
                this.BackgroundIndexer = new BackgroundIndexer();
                this.LastIndexStartTime = this.BackgroundIndexer.StartTimestamp;
                this.BackgroundIndexer.RunWorkerCompleted += this.BackgroundIndexerWorkCompleted;
                this.BackgroundIndexer.ProgressChanged += this.BackgroundIndexerProgressChanged;
                this.BackgroundIndexer.RunWorkerAsync(indexerParams);
            }
        }

        public void RunSearch(BackgroundSearcherParams searcherParams)
        {
            lock (this.searchingLock)
            {
                if (this.BackgroundSearcher != null)
                {
                    this.BackgroundSearcher.CancelAsync();
                }
                this.BackgroundSearcher = new BackgroundSearcher();
                this.LastSearchStartTime = this.BackgroundSearcher.StartTimestamp;
                this.BackgroundSearcher.RunWorkerCompleted += this.BackgroundSearcherWorkCompleted;
                this.BackgroundSearcher.RunWorkerAsync(searcherParams);
            }
        }

        private void BackgroundIndexerWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            lock (this.indexingLock)
            {
                this.NumOfIndexingThreads--;
                if (e.Cancelled)
                {
                    return;
                }
                var result = (IndexerResult)e.Result;
                if (result == null || !result.IsChanged || result.StartTimestamp != this.LastIndexStartTime)
                {
                    return;
                }
                Logger.Debug("Indexing ended");
                if (this.IndexerResultHandler != null)
                {
                    this.IndexerResultHandler(this, result);
                }
            }
        }

        private void BackgroundIndexerProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }

        private void BackgroundSearcherWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            lock (this.searchingLock)
            {
                if (e.Cancelled)
                {
                    return;
                }
                var result = (SearcherResult)e.Result;
                if (result == null || result.StartTimestamp != this.LastSearchStartTime)
                {
                    return;
                }
                Logger.Debug("Searching ended");
                if (this.SearcherResultHandler != null)
                {
                    this.SearcherResultHandler(this, result);
                }
            }
        }
    }
}
