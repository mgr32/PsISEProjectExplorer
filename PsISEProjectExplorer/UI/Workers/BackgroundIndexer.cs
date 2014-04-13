using NLog;
using PsISEProjectExplorer.Model;
using PsISEProjectExplorer.Model.DocHierarchy;
using PsISEProjectExplorer.Services;
using System;
using System.Collections.Generic;
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
            Logger.Info("Indexing started, pathsChanged: " + (indexerParams.PathsChanged == null ? "null" : String.Join(", ", indexerParams.PathsChanged)));

            DocumentHierarchySearcher newSearcher;
            if (indexerParams.PathsChanged == null || indexerParams.RootDirectory != indexerParams.DocumentHierarchyFactory.CurrentDocumentHierarchyPath)
            {
                newSearcher = indexerParams.DocumentHierarchyFactory.CreateDocumentHierarchy(indexerParams.RootDirectory, indexerParams.FilesPatternProvider);
            }
            else
            {
                newSearcher = indexerParams.DocumentHierarchyFactory.UpdateDocumentHierarchy(indexerParams.PathsChanged, indexerParams.FilesPatternProvider);
            }
            e.Result = new WorkerResult(this.StartTimestamp, newSearcher);
        }

       
    }
}
