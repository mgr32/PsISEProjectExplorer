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
            this.WorkerReportsProgress = true;
            this.WorkerSupportsCancellation = true;
        }

        private void RunIndexing(object sender, DoWorkEventArgs e)
        {
            var indexerParams = (BackgroundIndexerParams)e.Argument;
            Logger.Info("Indexing started, rootDir: " + indexerParams.RootDirectory + ", pathsChanged: " + (indexerParams.PathsChanged == null ? "null" : String.Join(", ", indexerParams.PathsChanged)));

            lock (this)
            {
                try
                {
                    IEnumerable<string> paths;
                    if (indexerParams.PathsChanged == null)
                    {
                        paths = new List<string> { indexerParams.RootDirectory };
                    } else
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
    }
}
