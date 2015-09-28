using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace PsISEProjectExplorer.UI.Workers
{
    public class BackgroundIndexer : BackgroundWorker
    {
        private static object BackgroundIndexerLock = new Object();

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public DateTime StartTimestamp { get; private set; }

        public BackgroundIndexer()
        {
            this.StartTimestamp = DateTime.Now;
            this.DoWork += RunIndexing;
            this.WorkerReportsProgress = true;
            this.WorkerSupportsCancellation = true;
        }

        // running in Indexing thread
        private void RunIndexing(object sender, DoWorkEventArgs e)
        {
            if (Thread.CurrentThread.Name == null)
            {
                Thread.CurrentThread.Name = "PsISEPE-Indexer";
            }
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
            }
        }

        // running in Indexing thread
        public void ReportProgressInCurrentThread(string path)
        {
            this.OnProgressChanged(new ProgressChangedEventArgs(0, path));
        }
    }
}
