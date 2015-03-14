using NLog;
using PsISEProjectExplorer.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.Services
{
    public class FileSystemChangeWatcher
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private FileSystemChangeNotifier FileSystemChangeNotifier { get; set; }

        private FileSystemWatcher Watcher { get; set; }

        private FilesPatternProvider FilesPatternProvider { get; set; }

        private string RootPath { get; set; }

        public FileSystemChangeWatcher(EventHandler<FileSystemChangedInfo> fileSystemChangedEvent)
        {
            this.FileSystemChangeNotifier = new FileSystemChangeNotifier("PsISEPE-FileSystemNotifierReindexWatcher");
            this.FileSystemChangeNotifier.FileSystemChanged += fileSystemChangedEvent;
            this.Watcher = new FileSystemWatcher();
        }

        public void StopWatching()
        {
            lock (FileSystemChangeNotifier)
            {
                this.Watcher.EnableRaisingEvents = false;
                this.FileSystemChangeNotifier.ClearChangePool();
                this.RootPath = null;
            }
        }

        public void Watch(string path, FilesPatternProvider filesPatternProvider)
        {
            lock (FileSystemChangeNotifier)
            {
                this.Watcher.EnableRaisingEvents = false;
                if (path != RootPath)
                {
                    this.FileSystemChangeNotifier.ClearChangePool();
                }
                this.RootPath = path;
                if (String.IsNullOrEmpty(path) || !Directory.Exists(path))
                {
                    return;
                }
                this.FilesPatternProvider = filesPatternProvider;
                this.Watcher.Path = path;
                this.Watcher.InternalBufferSize = 65536;
                this.Watcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.Security;
                this.Watcher.IncludeSubdirectories = true;
                this.Watcher.Changed += OnFileChanged;
                this.Watcher.Created += OnFileChanged;
                this.Watcher.Deleted += OnFileChanged;
                this.Watcher.Renamed += OnFileRenamed;
                this.Watcher.EnableRaisingEvents = true;
            }
        }

        // runs on a separate thread (from system)
        private void OnFileChanged(object source, FileSystemEventArgs e)
        {
            bool isDir = Directory.Exists(e.FullPath);
            if (isDir && !this.FilesPatternProvider.DoesDirectoryMatch(e.FullPath))
            {
                return;
            }
            if (isDir && e.ChangeType == WatcherChangeTypes.Changed)
            {
                return;
            }
            // if !isDir, it can be either a file, or a deleted directory
            if (!isDir && e.ChangeType != WatcherChangeTypes.Deleted && !this.FilesPatternProvider.DoesFileMatch(e.FullPath))
            {
                return;
            }
            Logger.Debug("File changed: " + e.FullPath);
            this.FileSystemChangeNotifier.AddChangePoolEntry(new ChangePoolEntry(e.FullPath, RootPath));
        }

        // runs on a separate thread (from system)
        private void OnFileRenamed(object source, RenamedEventArgs e)
        {
            Logger.Debug("File renamed: " + e.OldFullPath + " to " + e.FullPath);
            bool isDir = Directory.Exists(e.FullPath) || Directory.Exists(e.OldFullPath);
            if (isDir || this.FilesPatternProvider.DoesFileMatch(e.OldFullPath))
            {
                this.FileSystemChangeNotifier.AddChangePoolEntry(new ChangePoolEntry(e.OldFullPath, RootPath));
            }
            if ((isDir || this.FilesPatternProvider.DoesFileMatch(e.FullPath)) &&
                e.FullPath.ToLowerInvariant() != e.OldFullPath.ToLowerInvariant())
            {
                this.FileSystemChangeNotifier.AddChangePoolEntry(new ChangePoolEntry(e.FullPath, RootPath));
            }
        }
    }
}
