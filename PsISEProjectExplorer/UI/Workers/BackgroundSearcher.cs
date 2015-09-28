using NLog;
using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using System;
using System.ComponentModel;
using System.Threading;

namespace PsISEProjectExplorer.UI.Workers
{
    public class BackgroundSearcher : BackgroundWorker
    {
        private static object BackgroundSearcherLock = new Object();

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public DateTime StartTimestamp { get; private set; }

        public BackgroundSearcher()
        {
            this.StartTimestamp = DateTime.Now;
            this.DoWork += RunSearching;
            this.WorkerSupportsCancellation = true;
        }

        public void RunWorkerSync(BackgroundSearcherParams argument)
        {
            var args = new DoWorkEventArgs(argument);
            this.RunSearching(this, args);
            this.OnRunWorkerCompleted(new RunWorkerCompletedEventArgs(args.Result, null, args.Cancel));
        }

        private void RunSearching(object sender, DoWorkEventArgs e)
        {
            if (Thread.CurrentThread.Name == null)
            {
                Thread.CurrentThread.Name = "PsISEPE-Searcher";
            }
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
            Logger.Info("Searching started, path: {0}, text: {1} ", searcherParams.Path ?? "null", searcherParams.SearchOptions.SearchText);
            try
            {
                INode result = searcherParams.DocumentHierarchySearcher.GetDocumentHierarchyViewNodeProjection(searcherParams.Path, searcherParams.SearchOptions, this);
                e.Result = new SearcherResult(this.StartTimestamp, result, searcherParams.Path, searcherParams.SearchOptions);
            }
            catch (OperationCanceledException)
            {
                e.Cancel = true;
            }
        }
    }
}
