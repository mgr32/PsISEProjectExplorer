using Microsoft.PowerShell.Host.ISE;
using NLog;
using PsISEProjectExplorer.Model;
using PsISEProjectExplorer.Services;
using PsISEProjectExplorer.UI.IseIntegration;
using PsISEProjectExplorer.UI.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace PsISEProjectExplorer.Commands
{
    [Component]
    public class Bootstrap
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IseIntegrator iseIntegrator;

        private readonly IseFileReloader iseFileReloader;

        private readonly CommandExecutor commandExecutor;

        private readonly WorkspaceDirectoryModel workspaceDirectoryModel;

        private readonly DocumentHierarchyFactory documentHierarchyFactory;

        private readonly FileSystemChangeWatcher fileSystemChangeWatcher;

        public Bootstrap(IseIntegrator iseIntegrator, IseFileReloader iseFileReloader, CommandExecutor commandExecutor,
            WorkspaceDirectoryModel workspaceDirectoryModel, DocumentHierarchyFactory documentHierarchyFactory, FileSystemChangeWatcher fileSystemChangeWatcher)
        {
            this.iseIntegrator = iseIntegrator;
            this.iseFileReloader = iseFileReloader;
            this.commandExecutor = commandExecutor;
            this.workspaceDirectoryModel = workspaceDirectoryModel;
            this.documentHierarchyFactory = documentHierarchyFactory;
            this.fileSystemChangeWatcher = fileSystemChangeWatcher;
        }

        public void Start(ObjectModelRoot objectModelRoot)
        {
            this.iseIntegrator.setHostObject(objectModelRoot);
            this.workspaceDirectoryModel.PropertyChanged += this.RecreateSearchTreeOnWorkspaceDirectoryChanged;
            this.iseIntegrator.FileTabChanged += this.ResetWorkspaceOnFileTabChanged;
            this.fileSystemChangeWatcher.RegisterOnChangeCallback(this.ReindexOnFileSystemChanged);
            this.iseFileReloader.startWatching();
            this.commandExecutor.ExecuteWithParam<ResetWorkspaceDirectoryCommand, bool>(true);
        }

        private void ResetWorkspaceOnFileTabChanged(object sender, IseEventArgs args)
        {
            this.commandExecutor.ExecuteWithParam<ResetWorkspaceDirectoryCommand, bool>(false);
            this.commandExecutor.Execute<SyncWithActiveDocumentCommand>();
        }

        private void RecreateSearchTreeOnWorkspaceDirectoryChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentWorkspaceDirectory")
            {
                this.commandExecutor.Execute<RecreateSearchTreeCommand>();
            }
        }

        private void ReindexOnFileSystemChanged(object sender, FileSystemChangedInfo changedInfo)
        {
            var workspaceDirectory = this.workspaceDirectoryModel.CurrentWorkspaceDirectory;
            var pathsChanged = changedInfo.PathsChanged.Where(p => p.RootPath == workspaceDirectory).Select(p => p.PathChanged).ToList();
            if (!pathsChanged.Any())
            {
                return;
            }
            if (pathsChanged.Contains(workspaceDirectory, StringComparer.InvariantCultureIgnoreCase))
            {
                pathsChanged = null;
            }
            Logger.Debug("OnFileSystemChanged: " + (pathsChanged == null ? "root" : string.Join(",", pathsChanged)));
            this.commandExecutor.ExecuteWithParam<ReindexSearchTreeCommand, IEnumerable<string>>(pathsChanged);
        }
           
    }
}
