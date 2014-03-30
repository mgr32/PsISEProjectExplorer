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
            this.DoWork += RunIndexing;
        }

        private void RunIndexing(object sender, DoWorkEventArgs e)
        {
            var indexerParams = (BackgroundSearcherParams)e.Argument;
            if (indexerParams.DocumentHierarchySearcher == null) 
            {
                e.Result = null;
                return;
            }
            Logger.Info("Searching started, text: " + indexerParams.SearchText);
            INode result = indexerParams.DocumentHierarchySearcher.GetFilteredDocumentHierarchyNodes(indexerParams.SearchText, indexerParams.SearchOptions);
            e.Result = new WorkerResult(this.StartTimestamp, result);
        }
    }

}
