using NLog;
using PsISEProjectExplorer.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PsISEProjectExplorer.UI.Workers
{
    [Component]
    public class SearchRunner
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IList<BackgroundSearcher> BackgroundSearchers { get; set; }

        private EventHandler<SearcherResult> SearcherResultHandler { get; set; }

        private DocumentHierarchySearcher DocumentHierarchySearcher { get; set; }

        public SearchRunner(DocumentHierarchySearcher documentHierarchySearcher)
        {
            this.DocumentHierarchySearcher = documentHierarchySearcher;
            this.BackgroundSearchers = new List<BackgroundSearcher>();
        }

        public void RegisterHandlers(EventHandler<SearcherResult> searcherResultHandler)
        {
            this.SearcherResultHandler = searcherResultHandler;
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
            var searcher = new BackgroundSearcher(this.DocumentHierarchySearcher);
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
