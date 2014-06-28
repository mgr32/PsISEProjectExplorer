using NLog;
using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using System;
using System.ComponentModel;

namespace PsISEProjectExplorer.UI.Workers
{
    public class BackgroundSearcher : BackgroundWorker
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public DateTime StartTimestamp { get; private set; }

        public BackgroundSearcher()
        {
            this.StartTimestamp = DateTime.Now;
            this.DoWork += RunSearching;
            this.WorkerSupportsCancellation = true;
        }

        private void RunSearching(object sender, DoWorkEventArgs e)
        {
            var searcherParams = (BackgroundSearcherParams)e.Argument;
            if (searcherParams.DocumentHierarchySearcher == null) 
            {
                e.Result = null;
                return;
            }
            Logger.Info("Searching started, text: " + searcherParams.SearchText);
            try
            {
                INode result = searcherParams.DocumentHierarchySearcher.GetFilteredDocumentHierarchyNodes(searcherParams.SearchText, searcherParams.SearchOptions, this);
                e.Result = new SearcherResult(this.StartTimestamp, result);
            }
            catch (OperationCanceledException)
            {
                e.Cancel = true;
            }
        }
    }

}
