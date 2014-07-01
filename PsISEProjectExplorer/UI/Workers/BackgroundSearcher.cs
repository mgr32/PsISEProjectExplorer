using NLog;
using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using System;
using System.ComponentModel;

namespace PsISEProjectExplorer.UI.Workers
{
    public class BackgroundSearcher : BackgroundWorker
    {

        private static object BackgroundSearcherLock = new Object();
    
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public DateTime StartTimestamp { get; private set; }

        private EventHandler<bool> SearchingStateChangedHandler;

        public BackgroundSearcher(EventHandler<bool> searchingStateChangedHandler)
        {
            this.StartTimestamp = DateTime.Now;
            this.DoWork += RunSearching;
            this.WorkerSupportsCancellation = true;
            this.SearchingStateChangedHandler = searchingStateChangedHandler;
        }

        private void RunSearching(object sender, DoWorkEventArgs e)
        {
            var searcherParams = (BackgroundSearcherParams)e.Argument;
            if (searcherParams.DocumentHierarchySearcher == null) 
            {
                e.Result = null;
                return;
            }
            if (searcherParams.Path == null)
            {
                // lock is only for full (non-incremental) searches
                lock (BackgroundSearcherLock)
                {
                    RunActualSearch(searcherParams, e);
                }
            }
            else
            {
                RunActualSearch(searcherParams, e);
            }
            
        }

        private void RunActualSearch(BackgroundSearcherParams searcherParams, DoWorkEventArgs e)
        {
            if (this.CancellationPending)
            {
                e.Cancel = true;
                return;
            }
            Logger.Info(String.Format("Searching started, path: {0}, text: {1} ", searcherParams.Path, searcherParams.SearchOptions.SearchText));
            try
            {
                this.SearchingStateChangedHandler(this, true);
                INode result = searcherParams.DocumentHierarchySearcher.GetDocumentHierarchyViewNodeProjection(searcherParams.Path, searcherParams.SearchOptions, this);
                e.Result = new SearcherResult(this.StartTimestamp, result, searcherParams.Path, searcherParams.SearchOptions);
            }
            catch (OperationCanceledException)
            {
                e.Cancel = true;
            }
            finally
            {
                this.SearchingStateChangedHandler(this, false);
            }
        }
    }

}
