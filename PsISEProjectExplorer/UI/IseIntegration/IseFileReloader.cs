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
			IseIntegrator = iseIntegrator;
			IseFileWatchers = new Dictionary<string, IseFileWatcher>();
			PathsToIgnore = new HashSet<string>();
			FileSystemChangeNotifier = new FileSystemChangeNotifier("PsISEPE-FileSystemNotifierIseReloader");
			FileSystemChangeNotifier.FileSystemChanged += OnIseFileChangedBatch;
			IseIntegrator.AttachFileCollectionChangedHandler(OnIseFilesCollectionChanged);
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
                        if (IseFileWatchers.ContainsKey(path))
                        {
							IseFileWatchers[path].StopWatching();
							IseFileWatchers.Remove(path);
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
                        if (File.Exists(path) && !IseFileWatchers.ContainsKey(path))
                        {
							IseFileWatchers.Add(path, new IseFileWatcher(FileSystemChangeNotifier, path, newItem));
                        }
                        newItem.PropertyChanged -= OnIseFilePropertyChanged;
                        newItem.PropertyChanged += OnIseFilePropertyChanged;
                    }
                }
            }
        }

        private void RefreshWatchers()
        {
            foreach (var watcher in IseFileWatchers.Values)
            {
                watcher.StopWatching();
            }
			IseFileWatchers.Clear();
			OnIseFilesCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, IseIntegrator.OpenIseFiles.ToList()));
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
                lock (PathsToIgnore)
                {
                    if (file.IsSaved)
                    {
						PathsToIgnore.Add(file.FullPath);
                    }
                    else
                    {
						PathsToIgnore.Remove(file.FullPath);
                    }
                }
            }
            else if (e.PropertyName == "FullPath")
            {
				// on 'save as' we don't have to access to the old name - need to refresh everything
				RefreshWatchers();
            }
        }
        private void OnIseFileChangedBatch(object sender, FileSystemChangedInfo changedInfo)
        {
            foreach (var changePoolEntry in changedInfo.PathsChanged)
            {
                var pathChanged = changePoolEntry.PathChanged;
                bool pathIgnored;
                lock (PathsToIgnore)
                {
                    pathIgnored = PathsToIgnore.Remove(pathChanged);
                }
                if (!pathIgnored)
                {
					ReloadFileOpenInIse(changePoolEntry);
                }
            }
        }

        private void ReloadFileOpenInIse(ChangePoolEntry changeEntry)
        {
            string path = changeEntry.PathChanged;
            var iseFile = IseIntegrator.OpenIseFiles.FirstOrDefault(f => f.FullPath == path);
            if (iseFile == null)
            {
                return;
            }
            var fileExists = File.Exists(path);
            if (IseIntegrator.IsFileSaved(path))
            {
                if (fileExists)
                {
                    if (CompareFileContents(path, iseFile.Editor.Text))
                    {
                        return;
                    }
					IseIntegrator.GoToFile(path);
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
					IseIntegrator.CloseFile(path);
                    if (changeEntry.PathAfterRename != null)
                    {
						IseIntegrator.GoToFile(changeEntry.PathAfterRename);
                    }
                    else if (fileExists)
                    {
						IseIntegrator.GoToFile(path);
                    }
                }
            }
            else
            {
                string message;
                if (fileExists)
                {
					IseIntegrator.GoToFile(path);
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
