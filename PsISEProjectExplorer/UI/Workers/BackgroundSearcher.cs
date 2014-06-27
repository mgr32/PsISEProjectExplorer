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
            var indexerParams = (BackgroundSearcherParams)e.Argument;
            if (indexerParams.DocumentHierarchySearcher == null) 
            {
                e.Result = null;
                return;
            }
            Logger.Info("Searching started, text: " + indexerParams.SearchText);
            try
            {
                INode result = indexerParams.DocumentHierarchySearcher.GetFilteredDocumentHierarchyNodes(indexerParams.SearchText, indexerParams.SearchOptions, this);
                e.Result = new SearcherResult(this.StartTimestamp, result);
            }
            catch (OperationCanceledException)
            {
                e.Cancel = true;
            }
        }
    }

}
