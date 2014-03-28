using NLog;
using PsISEProjectExplorer.Model.DocHierarchy;
using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using PsISEProjectExplorer.Services;
using PsISEProjectExplorer.UI.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.UI.Workers
{
    public class BackgroundIndexer : BackgroundWorker
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public DateTime StartTimestamp { get; private set; }

        public BackgroundIndexer()
        {
            this.StartTimestamp = DateTime.Now;
            this.DoWork += new DoWorkEventHandler(doWork);
        }

        private void doWork(object sender, DoWorkEventArgs e)
        {
            BackgroundIndexerParams indexerParams = (BackgroundIndexerParams)e.Argument;
            DocumentHierarchy docHierarchy = null;
            DocumentHierarchySearcher newSearcher = null;
            logger.Info("Indexing started");
            if (indexerParams.PathsChanged == null)
            {
                docHierarchy = indexerParams.DocumentHierarchyIndexer.CreateDocumentHierarchy(indexerParams.RootDirectory);
                newSearcher = new DocumentHierarchySearcher(docHierarchy);
            }
            else
            {
                docHierarchy = indexerParams.DocumentHierarchyIndexer.GetDocumentHierarchy(indexerParams.RootDirectory);
                bool changed = indexerParams.DocumentHierarchyIndexer.UpdateDocumentHierarchy(docHierarchy, indexerParams.PathsChanged);
                if (changed)
                {
                    newSearcher = new DocumentHierarchySearcher(docHierarchy);
                }
            }

            e.Result = new WorkerResult(this.StartTimestamp, newSearcher);
        }

    }
}
