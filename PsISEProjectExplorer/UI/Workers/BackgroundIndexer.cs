using NLog;
using PsISEProjectExplorer.Model.DocHierarchy;
using PsISEProjectExplorer.Services;
using System;
using System.ComponentModel;

namespace PsISEProjectExplorer.UI.Workers
{
    public class BackgroundIndexer : BackgroundWorker
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public DateTime StartTimestamp { get; private set; }

        public BackgroundIndexer()
        {
            this.StartTimestamp = DateTime.Now;
            this.DoWork += RunIndexing;
        }

        private void RunIndexing(object sender, DoWorkEventArgs e)
        {
            var indexerParams = (BackgroundIndexerParams)e.Argument;
            DocumentHierarchySearcher newSearcher = null;
            Logger.Info("Indexing started");
            if (indexerParams.PathsChanged == null)
            {
                newSearcher = this.CreateNewDocHierarchy(indexerParams.DocumentHierarchyFactory, indexerParams.RootDirectory);
            }
            else
            {
                DocumentHierarchy docHierarchy = indexerParams.DocumentHierarchyFactory.GetDocumentHierarchy(indexerParams.RootDirectory);
                if (docHierarchy == null)
                {
                    newSearcher = this.CreateNewDocHierarchy(indexerParams.DocumentHierarchyFactory, indexerParams.RootDirectory);
                }
                else
                {
                    bool changed = indexerParams.DocumentHierarchyFactory.UpdateDocumentHierarchy(docHierarchy, indexerParams.PathsChanged);
                    if (changed)
                    {
                        newSearcher = new DocumentHierarchySearcher(docHierarchy);
                    }
                }
            }
            e.Result = new WorkerResult(this.StartTimestamp, newSearcher);
        }

        private DocumentHierarchySearcher CreateNewDocHierarchy(DocumentHierarchyFactory factory, string rootDirectory)
        {
            var docHierarchy = factory.CreateDocumentHierarchy(rootDirectory);
            return new DocumentHierarchySearcher(docHierarchy);
        }

    }
}
