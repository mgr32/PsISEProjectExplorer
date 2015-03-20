using NLog;
using PsISEProjectExplorer.Model;
using System;
using System.IO;

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
			FileSystemChangeNotifier = new FileSystemChangeNotifier("PsISEPE-FileSystemNotifierReindexWatcher");
			FileSystemChangeNotifier.FileSystemChanged += fileSystemChangedEvent;
			Watcher = new FileSystemWatcher();
        }

        public void StopWatching()
        {
            lock (FileSystemChangeNotifier)
            {
				Watcher.EnableRaisingEvents = false;
				FileSystemChangeNotifier.ClearChangePool();
				RootPath = null;
            }
        }

        public void Watch(string path, FilesPatternProvider filesPatternProvider)
        {
            lock (FileSystemChangeNotifier)
            {
				Watcher.EnableRaisingEvents = false;
                if (path != RootPath)
                {
					FileSystemChangeNotifier.ClearChangePool();
                }
				RootPath = path;
                if (String.IsNullOrEmpty(path) || !Directory.Exists(path))
                {
                    return;
                }
				FilesPatternProvider = filesPatternProvider;
				Watcher.Path = path;
				Watcher.InternalBufferSize = 65536;
				Watcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.Security;
				Watcher.IncludeSubdirectories = true;
				Watcher.Changed += OnFileChanged;
				Watcher.Created += OnFileChanged;
				Watcher.Deleted += OnFileChanged;
				Watcher.Renamed += OnFileRenamed;
				Watcher.EnableRaisingEvents = true;
            }
        }

        // runs on a separate thread (from system)
        private void OnFileChanged(object source, FileSystemEventArgs e)
        {
            bool isDir = Directory.Exists(e.FullPath);
            if (isDir && !FilesPatternProvider.DoesDirectoryMatch(e.FullPath))
            {
                return;
            }
            if (isDir && e.ChangeType == WatcherChangeTypes.Changed)
            {
                return;
            }
            // if !isDir, it can be either a file, or a deleted directory
            if (!isDir && e.ChangeType != WatcherChangeTypes.Deleted && !FilesPatternProvider.DoesFileMatch(e.FullPath))
            {
                return;
            }
            Logger.Debug("File changed: " + e.FullPath);
			FileSystemChangeNotifier.AddChangePoolEntry(new ChangePoolEntry(e.FullPath, RootPath));
        }

        // runs on a separate thread (from system)
        private void OnFileRenamed(object source, RenamedEventArgs e)
        {
            Logger.Debug("File renamed: " + e.OldFullPath + " to " + e.FullPath);
            bool isDir = Directory.Exists(e.FullPath) || Directory.Exists(e.OldFullPath);
            if (isDir || FilesPatternProvider.DoesFileMatch(e.OldFullPath))
            {
				FileSystemChangeNotifier.AddChangePoolEntry(new ChangePoolEntry(e.OldFullPath, RootPath));
            }
            if ((isDir || FilesPatternProvider.DoesFileMatch(e.FullPath)) &&
                e.FullPath.ToLowerInvariant() != e.OldFullPath.ToLowerInvariant())
            {
				FileSystemChangeNotifier.AddChangePoolEntry(new ChangePoolEntry(e.FullPath, RootPath));
            }
        }
    }
}
