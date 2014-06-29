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

namespace PsISEProjectExplorer.UI.ViewModel
{
    public class IndexingSearchingModel : BaseViewModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private bool indexingInProgress;

        public bool IndexingInProgress
        {
            get
            {
                return this.indexingInProgress;
            }
            private set
            {
                this.indexingInProgress = value;
                this.OnPropertyChanged("IndexingInProgress");
            }
        }

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
            if (this.BackgroundIndexer != null)
            {
                this.BackgroundIndexer.CancelAsync();
            }
            
            this.IndexingInProgress = true;
            this.BackgroundIndexer = new BackgroundIndexer(this.IndexingStateChangedHandler);
            this.BackgroundIndexer.RunWorkerCompleted += this.BackgroundIndexerWorkCompleted;
            this.BackgroundIndexer.ProgressChanged += this.BackgroundIndexerProgressChanged;
            this.BackgroundIndexer.RunWorkerAsync(indexerParams);
        }

        private void IndexingStateChangedHandler(object sender, bool value)
        {
            this.IndexingInProgress = value;
        }

        public void RunSearch(BackgroundSearcherParams searcherParams)
        {
            this.BackgroundSearcher = new BackgroundSearcher();
            this.BackgroundSearcher.RunWorkerCompleted += this.BackgroundSearcherWorkCompleted;
            this.BackgroundSearcher.RunWorkerAsync(searcherParams);
        }

        private void BackgroundIndexerWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.IndexingInProgress = false;
            if (e.Cancelled)
            {
                return;
            }
            var result = (IndexerResult)e.Result;
            if (result == null || !result.IsChanged)
            {
                return;
            }
            Logger.Debug("Indexing ended");
            if (this.IndexerResultHandler != null)
            {
                this.IndexerResultHandler(this, result);
            }
            
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
                return;
            }
            var result = (SearcherResult)e.Result;
            if (result == null)
            {
                return;
            }
            Logger.Debug(String.Format("Searching ended, path: {0}", result.ResultNode != null ? result.ResultNode.Path : "null"));
            if (this.SearcherResultHandler != null)
            {
                this.SearcherResultHandler(this, result);
            }
            
        }
    }
}
