
using NLog;
using PsISEProjectExplorer.Services;
using PsISEProjectExplorer.UI.ViewModel;
using PsISEProjectExplorer.UI.Workers;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PsISEProjectExplorer.Commands
{
    [Component]
    public class ReindexSearchTreeCommand : ParameterizedCommand<IEnumerable<string>>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IList<BackgroundIndexer> backgroundIndexers;

        private readonly MainViewModel mainViewModel;

        private readonly DocumentHierarchyFactory documentHierarchyFactory;

        private readonly WorkspaceDirectoryModel workspaceDirectoryModel;

        private readonly ClearTreeViewCommand clearTreeViewCommand;

        private readonly RunSearchCommand runSearchCommand;

        private readonly FilesPatternProvider filesPatternProvider;

        private readonly SyncWithActiveDocumentCommand syncWithActiveDocumentCommand;

        public ReindexSearchTreeCommand(MainViewModel mainViewModel, DocumentHierarchyFactory documentHierarchyFactory, WorkspaceDirectoryModel workspaceDirectoryModel,
            ClearTreeViewCommand clearTreeViewCommand, RunSearchCommand runSearchCommand, FilesPatternProvider filesPatternProvider, SyncWithActiveDocumentCommand syncWithActiveDocumentCommand)
        {
            this.mainViewModel = mainViewModel;
            this.documentHierarchyFactory = documentHierarchyFactory;
            this.workspaceDirectoryModel = workspaceDirectoryModel;
            this.clearTreeViewCommand = clearTreeViewCommand;
            this.runSearchCommand = runSearchCommand;
            this.filesPatternProvider = filesPatternProvider;
            this.syncWithActiveDocumentCommand = syncWithActiveDocumentCommand;
            this.backgroundIndexers = new List<BackgroundIndexer>();
        }

        public void Execute(IEnumerable<string> pathsChanged)
        {
            if (pathsChanged == null)
            {
                this.documentHierarchyFactory.CreateDocumentHierarchy(this.workspaceDirectoryModel.CurrentWorkspaceDirectory, this.mainViewModel.AnalyzeDocumentContents);
                this.clearTreeViewCommand.Execute();
                this.syncWithActiveDocumentCommand.Execute(true);
            }
            this.mainViewModel.AddNumOfIndexingThreads(1);
            var indexerParams = new BackgroundIndexerParams(this.documentHierarchyFactory, this.workspaceDirectoryModel.CurrentWorkspaceDirectory, pathsChanged, this.filesPatternProvider);
            this.ReindexSearchTree(indexerParams);
        }

        // running in UI thread
        private void ReindexSearchTree(BackgroundIndexerParams indexerParams)
        {
            if (indexerParams.PathsChanged == null)
            {
                lock (this.backgroundIndexers)
                {
                    foreach (var ind in this.backgroundIndexers)
                    {
                        ind.CancelAsync();
                    }
                    this.backgroundIndexers.Clear();
                }
            }

            var indexer = new BackgroundIndexer();
            indexer.RunWorkerCompleted += this.BackgroundIndexerWorkCompleted;
            indexer.ProgressChanged += this.BackgroundIndexerProgressChanged;
            indexer.RunWorkerAsync(indexerParams);
            lock (this.backgroundIndexers)
            {
                this.backgroundIndexers.Add(indexer);
            }
        }

        // running in UI thread
        private void BackgroundIndexerWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var indexer = sender as BackgroundIndexer;
            if (indexer != null)
            {
                lock (this.backgroundIndexers)
                {
                    this.backgroundIndexers.Remove(indexer);
                }
            }
            this.mainViewModel.AddNumOfIndexingThreads(-1);
        }

        // running in Indexing thread
        private void BackgroundIndexerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Logger.Debug(String.Format("Indexer progress, path: {0}", (string)e.UserState));
            this.runSearchCommand.Execute((string)e.UserState);
        }
    }
}
