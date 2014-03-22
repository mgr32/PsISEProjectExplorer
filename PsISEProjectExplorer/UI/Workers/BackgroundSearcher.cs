using NLog;
using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.UI.Workers
{
    public class BackgroundSearcher : BackgroundWorker
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public DateTime StartTimestamp { get; private set; }

        public BackgroundSearcher()
        {
            this.StartTimestamp = DateTime.Now;
            this.DoWork += new DoWorkEventHandler(doWork);
        }

        private void doWork(object sender, DoWorkEventArgs e)
        {
            BackgroundSearcherParams indexerParams = (BackgroundSearcherParams)e.Argument;
            if (indexerParams.DocumentHierarchySearcher == null) 
            {
                e.Result = null;
                return;
            }
            logger.Info("Searching started, text: " + indexerParams.SearchText);
            INode result = indexerParams.DocumentHierarchySearcher.GetFilteredDocumentHierarchyNodes(indexerParams.SearchText, indexerParams.SearchOptions);
            e.Result = new WorkerResult(this.StartTimestamp, result);
        }
    }

}
