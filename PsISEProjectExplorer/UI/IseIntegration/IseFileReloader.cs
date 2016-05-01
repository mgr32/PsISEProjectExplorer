using Microsoft.PowerShell.Host.ISE;
using PsISEProjectExplorer.Model;
using PsISEProjectExplorer.Services;
using PsISEProjectExplorer.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace PsISEProjectExplorer.UI.IseIntegration
{
    [Component]
    public class IseFileReloader
    {
        private readonly IseIntegrator iseIntegrator;

        private readonly IDictionary<string, IseFileWatcher> iseFileWatchers;

        private readonly ISet<string> pathsToIgnore;

        private FileSystemChangeNotifier fileSystemChangeNotifier;

        private readonly FileSystemOperationsService fileSystemOperationsService;

        private readonly MessageBoxHelper messageBoxHelper;

        public IseFileReloader(IseIntegrator iseIntegrator, FileSystemOperationsService fileSystemOperationsService, MessageBoxHelper messageBoxHelper)
        {
            this.iseIntegrator = iseIntegrator;
            this.fileSystemOperationsService = fileSystemOperationsService;
            this.messageBoxHelper = messageBoxHelper;
            this.iseFileWatchers = new Dictionary<string, IseFileWatcher>();
            this.pathsToIgnore = new HashSet<string>();
        }

        public void startWatching()
        {
            this.fileSystemChangeNotifier = new FileSystemChangeNotifier("PsISEPE-FileSystemNotifierIseReloader", this.fileSystemOperationsService);
            this.fileSystemChangeNotifier.FileSystemChanged += OnIseFileChangedBatch;
            this.iseIntegrator.AttachFileCollectionChangedHandler(this.OnIseFilesCollectionChanged);
        }

        private void OnIseFilesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (ISEFile oldItem in e.OldItems)
                {
                    if (e.NewItems == null || !e.NewItems.Contains(oldItem))
                    {
                        var path = oldItem.FullPath;
                        if (this.iseFileWatchers.ContainsKey(path))
                        {
                            this.iseFileWatchers[path].StopWatching();
                            this.iseFileWatchers.Remove(path);
                        }
                        oldItem.PropertyChanged -= OnIseFilePropertyChanged;
                    }
                }
            }

            if (e.NewItems != null)
            {
                foreach (ISEFile newItem in e.NewItems)
                {
                    if (e.OldItems == null || !e.OldItems.Contains(newItem))
                    {
                        var path = newItem.FullPath;
                        if (File.Exists(path) && !this.iseFileWatchers.ContainsKey(path))
                        {
                            this.iseFileWatchers.Add(path, new IseFileWatcher(this.fileSystemChangeNotifier, path, newItem));
                        }
                        newItem.PropertyChanged -= OnIseFilePropertyChanged;
                        newItem.PropertyChanged += OnIseFilePropertyChanged;
                    }
                }
            }
        }

        private void RefreshWatchers()
        {
            foreach (var watcher in this.iseFileWatchers.Values)
            {
                watcher.StopWatching();
            }
            this.iseFileWatchers.Clear();
            this.OnIseFilesCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, this.iseIntegrator.OpenIseFiles.ToList()));
        }

        private void OnIseFilePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ISEFile file = sender as ISEFile;
            if (file == null)
            {
                return;
            }
            if (e.PropertyName == "IsSaved")
            {
                lock (this.pathsToIgnore)
                {
                    if (file.IsSaved)
                    {
                        this.pathsToIgnore.Add(file.FullPath);
                    }
                    else
                    {
                        this.pathsToIgnore.Remove(file.FullPath);
                    }
                }
            }
            else if (e.PropertyName == "FullPath")
            {
                // on 'save as' we don't have to access to the old name - need to refresh everything
                this.RefreshWatchers();
            }
        }

        private void OnIseFileChangedBatch(object sender, FileSystemChangedInfo changedInfo)
        {
            foreach (var changePoolEntry in changedInfo.PathsChanged)
            {
                var pathChanged = changePoolEntry.PathChanged;
                bool pathIgnored;
                lock (this.pathsToIgnore)
                {
                    pathIgnored = this.pathsToIgnore.Remove(pathChanged);
                }
                if (!pathIgnored)
                {
                    this.ReloadFileOpenInIse(changePoolEntry);
                }
            }
        }

        private void ReloadFileOpenInIse(ChangePoolEntry changeEntry)
        {
            string path = changeEntry.PathChanged;
            var iseFile = this.iseIntegrator.OpenIseFiles.FirstOrDefault(f => f.FullPath == path);
            if (iseFile == null)
            {
                return;
            }
            var fileExists = File.Exists(path);
            if (this.iseIntegrator.IsFileSaved(path))
            {
                if (fileExists)
                {
                    if (this.CompareFileContents(path, iseFile.Editor.Text))
                    {
                        return;
                    }
                    this.iseIntegrator.GoToFile(path);
                }
                string question;
                if (fileExists)
                {
                    question = String.Format("File '{0}' has been modified by another program.\n\nDo you want to reload it?", path);
                }
                else if (changeEntry.PathAfterRename != null)
                {
                    question = String.Format("File '{0}' has been moved to '{1}'.\n\nDo you want to reload it?", path, changeEntry.PathAfterRename);
                }
                else
                {
                    question = String.Format("File '{0}' has been deleted.\n\nDo you want to close it?", path);
                }
                if (this.messageBoxHelper.ShowQuestion("Reload file", question))
                {
                    this.iseIntegrator.CloseFile(path);
                    if (changeEntry.PathAfterRename != null)
                    {
                        this.iseIntegrator.GoToFile(changeEntry.PathAfterRename);
                    }
                    else if (fileExists)
                    {
                        this.iseIntegrator.GoToFile(path);
                    }
                }
            }
            else
            {
                string message;
                if (fileExists)
                {
                    this.iseIntegrator.GoToFile(path);
                    message = String.Format("File '{0}' has been modified by another program.\n\nSince the file had been changed in ISE editor, you will need to reload it manually.", path);
                }
                else if (changeEntry.PathAfterRename != null)
                {
                    message = String.Format("File '{0}' has been moved to '{1}.\n\nSince the file had been changed in ISE editor, you will need to reload it manually.", path, changeEntry.PathAfterRename);
                }
                else
                {
                    message = String.Format("File '{0}' has been deleted. Since the file had been changed in ISE editor, you will need to reload it manually.", path);
                }
                this.messageBoxHelper.ShowInfo(message);
            }
        }

        private bool CompareFileContents(string path, string fileContentsToCompare)
        {
            string fileText = null;
            try
            {
                fileText = File.ReadAllText(path);
            }
            catch (Exception)
            {
                return false;
            }
            return fileText == fileContentsToCompare;
        }
    }
}
