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
    public class IseFileReloader
    {

        private IseIntegrator IseIntegrator { get; set; }

        private IDictionary<string, IseFileWatcher> IseFileWatchers { get; set; }

        private ISet<string> PathsToIgnore { get; set; }

        private FileSystemChangeNotifier FileSystemChangeNotifier { get; set; }

        public IseFileReloader(IseIntegrator iseIntegrator)
        {
            this.IseIntegrator = iseIntegrator;
            this.IseFileWatchers = new Dictionary<string, IseFileWatcher>();
            this.PathsToIgnore = new HashSet<string>();
            this.FileSystemChangeNotifier = new FileSystemChangeNotifier("PsISEPE-FileSystemNotifierIseReloader");
            this.FileSystemChangeNotifier.FileSystemChanged += OnIseFileChangedBatch;
            this.IseIntegrator.AttachFileCollectionChangedHandler(this.OnIseFilesCollectionChanged);
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
                        if (this.IseFileWatchers.ContainsKey(path))
                        {
                            this.IseFileWatchers[path].StopWatching();
                            this.IseFileWatchers.Remove(path);
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
                        if (File.Exists(path) && !this.IseFileWatchers.ContainsKey(path))
                        {
                            this.IseFileWatchers.Add(path, new IseFileWatcher(this.FileSystemChangeNotifier, path, newItem));
                        }
                        newItem.PropertyChanged -= OnIseFilePropertyChanged;
                        newItem.PropertyChanged += OnIseFilePropertyChanged;
                    }
                }
            }
        }

        private void RefreshWatchers()
        {
            foreach (var watcher in this.IseFileWatchers.Values)
            {
                watcher.StopWatching();
            }
            this.IseFileWatchers.Clear();
            this.OnIseFilesCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, this.IseIntegrator.OpenIseFiles.ToList()));
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
                lock (this.PathsToIgnore)
                {
                    if (file.IsSaved)
                    {
                        this.PathsToIgnore.Add(file.FullPath);
                    }
                    else
                    {
                        this.PathsToIgnore.Remove(file.FullPath);
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
                lock (this.PathsToIgnore)
                {
                    pathIgnored = this.PathsToIgnore.Remove(pathChanged);
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
            var iseFile = this.IseIntegrator.OpenIseFiles.FirstOrDefault(f => f.FullPath == path);
            if (iseFile == null)
            {
                return;
            }
            var fileExists = File.Exists(path);
            if (this.IseIntegrator.IsFileSaved(path))
            {
                if (fileExists)
                {
                    if (this.CompareFileContents(path, iseFile.Editor.Text))
                    {
                        return;
                    }
                    this.IseIntegrator.GoToFile(path);
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
                if (MessageBoxHelper.ShowQuestion("Reload file", question))
                {
                    this.IseIntegrator.CloseFile(path);
                    if (changeEntry.PathAfterRename != null)
                    {
                        this.IseIntegrator.GoToFile(changeEntry.PathAfterRename);
                    }
                    else if (fileExists)
                    {
                        this.IseIntegrator.GoToFile(path);
                    }
                }
            }
            else
            {
                string message;
                if (fileExists)
                {
                    this.IseIntegrator.GoToFile(path);
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
                MessageBoxHelper.ShowInfo(message);
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
