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
        private static object BackgroundIndexerLock = new Object();

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public DateTime StartTimestamp { get; private set; }

        private EventHandler<bool> IndexingStateChangedHandler;

        public BackgroundIndexer(EventHandler<bool> indexingStateChangedHandler)
        {
            this.StartTimestamp = DateTime.Now;
            this.DoWork += RunIndexing;
            this.WorkerReportsProgress = true;
            this.WorkerSupportsCancellation = true;
            this.IndexingStateChangedHandler = indexingStateChangedHandler;
        }

        private void RunIndexing(object sender, DoWorkEventArgs e)
        {
            var indexerParams = (BackgroundIndexerParams)e.Argument;
            Logger.Info("Indexing started, rootDir: " + indexerParams.RootDirectory + ", pathsChanged: " + (indexerParams.PathsChanged == null ? "null" : String.Join(", ", indexerParams.PathsChanged)));
            lock (BackgroundIndexerLock)
            {
                if (this.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                try
                {
                    this.IndexingStateChangedHandler(this, true);
                    IEnumerable<string> paths;
                    if (indexerParams.PathsChanged == null)
                    {
                        paths = new List<string> { indexerParams.RootDirectory };
                    }
                    else
                    {
                        paths = indexerParams.PathsChanged;
                    }
                    var isChanged = indexerParams.DocumentHierarchyFactory.UpdateDocumentHierarchy(paths, indexerParams.FilesPatternProvider, this);
                    e.Result = new IndexerResult(this.StartTimestamp, isChanged);
                }
                catch (OperationCanceledException)
                {
                    e.Cancel = true;
                }
                finally
                {
                    this.IndexingStateChangedHandler(this, false);
                }
            }
        }
    }
}
