using System.Threading;
using PsISEProjectExplorer.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.Services
{
    public static class FileSystemChangeNotifier
    {
        public static event EventHandler<FileSystemChangedInfo> FileSystemChanged;

        private static readonly ISet<string> ChangePool = new HashSet<string>();

        private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        private static readonly FileSystemWatcher Watcher = new FileSystemWatcher();

        static FileSystemChangeNotifier()
        {
            Task.Factory.StartNew(ChangeNotifier);
        }

        public static void Watch(string path)
        {
            Watcher.EnableRaisingEvents = false;
            if (String.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                return;
            }
            Watcher.Path = path;
            Watcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.Security;
            Watcher.IncludeSubdirectories = true;
            Watcher.Changed += OnFileChanged;
            Watcher.Created += OnFileChanged;
            Watcher.Deleted += OnFileChanged;
            Watcher.Renamed += OnFileRenamed;
            Watcher.EnableRaisingEvents = true;
        }

        // runs on a separate thread (from system)
        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            bool isDir = Directory.Exists(e.FullPath);
            if (isDir && e.ChangeType == WatcherChangeTypes.Changed)
            {
                return;
            }
            if (!isDir && !FilesPatternProvider.PowershellFilesRegex.IsMatch(e.FullPath))
            {
                return;
            }
            lock (ChangePool)
            {
                ChangePool.Add(e.FullPath);
            }
        }

        // runs on a separate thread (from system)
        private static void OnFileRenamed(object source, RenamedEventArgs e)
        {
            bool isDir = Directory.Exists(e.FullPath) || Directory.Exists(e.OldFullPath);
            lock (ChangePool)
            {
                if (isDir || FilesPatternProvider.PowershellFilesRegex.IsMatch(e.OldFullPath))
                {
                    ChangePool.Add(e.OldFullPath);
                }
                if ((isDir || FilesPatternProvider.PowershellFilesRegex.IsMatch(e.FullPath)) &&
                    e.FullPath.ToLowerInvariant() != e.OldFullPath.ToLowerInvariant())
                {
                    ChangePool.Add(e.FullPath);
                }
            }
        }

        // runs on a separate thread (created in constructor)
        private static void ChangeNotifier()
        {
            while (true)
            {
                Thread.Sleep(100);
                if (CancellationTokenSource.Token.IsCancellationRequested)
                {
                    return;
                }
                lock (ChangePool)
                {
                    if (ChangePool.Any())
                    {
                        IList<string> pathsChanged = new List<string>(ChangePool);
                        var changedInfo = new FileSystemChangedInfo(pathsChanged);
                        FileSystemChanged(null, changedInfo);
                        ChangePool.Clear();
                    }
                }
            }
        }
    }
}
