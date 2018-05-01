using NLog;
using PsISEProjectExplorer.Model;
using System;
using System.IO;

namespace PsISEProjectExplorer.Services
{
    [Component]
    public class FileSystemChangeWatcher
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private FileSystemChangeNotifier fileSystemChangeNotifier;

        private FileSystemWatcher watcher;

        private readonly FilesPatternProvider filesPatternProvider;

        private readonly FileSystemOperationsService fileSystemOperationsService;

        private string RootPath { get; set; }

        public FileSystemChangeWatcher(FileSystemOperationsService fileSystemOperationsService, FilesPatternProvider filesPatternProvider)
        {
            this.fileSystemOperationsService = fileSystemOperationsService;
            this.filesPatternProvider = filesPatternProvider;
        }

        public void RegisterOnChangeCallback(EventHandler<FileSystemChangedInfo> fileSystemChangedEvent)
        {
            this.fileSystemChangeNotifier = new FileSystemChangeNotifier("PsISEPE-FileSystemNotifierReindexWatcher", this.fileSystemOperationsService);
            this.fileSystemChangeNotifier.FileSystemChanged += fileSystemChangedEvent;
            this.watcher = new FileSystemWatcher();
        }

        public void StopWatching()
        {
            lock (fileSystemChangeNotifier)
            {
                this.watcher.EnableRaisingEvents = false;
                this.fileSystemChangeNotifier.ClearChangePool();
                this.RootPath = null;
            }
        }

        public void Watch(string path)
        {
            lock (fileSystemChangeNotifier)
            {
                this.watcher.EnableRaisingEvents = false;
                if (path != RootPath)
                {
                    this.fileSystemChangeNotifier.ClearChangePool();
                }
                this.RootPath = path;
                if (String.IsNullOrEmpty(path) || !Directory.Exists(path))
                {
                    return;
                }
                this.watcher.Path = path;
                this.watcher.InternalBufferSize = 65536;
                this.watcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.Security;
                this.watcher.IncludeSubdirectories = true;
                this.watcher.Changed += OnFileChanged;
                this.watcher.Created += OnFileChanged;
                this.watcher.Deleted += OnFileChanged;
                this.watcher.Renamed += OnFileRenamed;
                this.watcher.EnableRaisingEvents = true;
            }
        }

        // runs on a separate thread (from system)
        private void OnFileChanged(object source, FileSystemEventArgs e)
        {
            if (this.filesPatternProvider.IsExcludedByUser(e.FullPath))
            {
                return;
            }
            bool isDir = Directory.Exists(e.FullPath);
            if (isDir && !this.filesPatternProvider.DoesDirectoryMatch(e.FullPath))
            {
                return;
            }
            if (isDir && e.ChangeType == WatcherChangeTypes.Changed)
            {
                return;
            }
            // if !isDir, it can be either a file, or a deleted directory
            if (!isDir && e.ChangeType == WatcherChangeTypes.Deleted && this.filesPatternProvider.DoesDirectoryMatch(e.FullPath))
            {
                this.fileSystemChangeNotifier.AddChangePoolEntry(new ChangePoolEntry(e.FullPath, RootPath));
                return;
            }
            if (!isDir && !this.filesPatternProvider.DoesFileMatch(e.FullPath))
            {
                return;
            }
            Logger.Debug("File changed: " + e.FullPath);
            this.fileSystemChangeNotifier.AddChangePoolEntry(new ChangePoolEntry(e.FullPath, RootPath));
        }

        // runs on a separate thread (from system)
        private void OnFileRenamed(object source, RenamedEventArgs e)
        {
            Logger.Debug("File renamed: " + e.OldFullPath + " to " + e.FullPath);
            bool isDir = Directory.Exists(e.FullPath) || Directory.Exists(e.OldFullPath);
            if (!this.filesPatternProvider.IsExcludedByUser(e.OldFullPath) && (isDir || this.filesPatternProvider.DoesFileMatch(e.OldFullPath)))
            {
                this.fileSystemChangeNotifier.AddChangePoolEntry(new ChangePoolEntry(e.OldFullPath, RootPath));
            }
            if (!this.filesPatternProvider.IsExcludedByUser(e.FullPath) && (isDir || this.filesPatternProvider.DoesFileMatch(e.FullPath)) &&
                e.FullPath.ToLowerInvariant() != e.OldFullPath.ToLowerInvariant())
            {
                this.fileSystemChangeNotifier.AddChangePoolEntry(new ChangePoolEntry(e.FullPath, RootPath));
            }
        }
    }
}
