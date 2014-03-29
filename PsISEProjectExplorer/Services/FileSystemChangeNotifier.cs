using NLog;
using PsISEProjectExplorer.Model;
using PsISEProjectExplorer.Model.DocHierarchy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.Services
{
    public static class FileSystemChangeNotifier
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static event EventHandler<FileSystemChangedInfo> FileSystemChanged;

        private static ISet<string> changePool = new HashSet<string>();

        private static Task changeNotifyTask;

        private static FileSystemWatcher watcher = new FileSystemWatcher();

        public static void Watch(string path)
        {
            watcher.EnableRaisingEvents = false;
            if (String.IsNullOrEmpty(path))
            {
                return;
            }
            changeNotifyTask = Task.Factory.StartNew(() => ChangeNotifier());
            watcher.Path = path;
            watcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.Security;
            watcher.IncludeSubdirectories = true;
            watcher.Changed += new FileSystemEventHandler(OnFileChanged);
            watcher.Created += new FileSystemEventHandler(OnFileChanged);
            watcher.Deleted += new FileSystemEventHandler(OnFileChanged);
            watcher.Renamed += new RenamedEventHandler(OnFileRenamed);
            watcher.EnableRaisingEvents = true;
        }

        // runs on a separate thread (from system)
        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            bool isDir = Directory.Exists(e.FullPath);
            if (isDir && e.ChangeType == WatcherChangeTypes.Changed)
            {
                return;
            }
            if (!isDir && !FilesPatternProvider.POWERSHELL_FILES_REGEX.IsMatch(e.FullPath))
            {
                return;
            }
            lock (changePool)
            {
                changePool.Add(e.FullPath);
            }
        }

        // runs on a separate thread (from system)
        private static void OnFileRenamed(object source, RenamedEventArgs e)
        {
            bool isDir = Directory.Exists(e.FullPath) || Directory.Exists(e.OldFullPath);
            lock (changePool)
            {
                if (isDir || FilesPatternProvider.POWERSHELL_FILES_REGEX.IsMatch(e.OldFullPath))
                {
                    changePool.Add(e.OldFullPath);
                }
                if ((isDir || FilesPatternProvider.POWERSHELL_FILES_REGEX.IsMatch(e.FullPath)) &&
                    e.FullPath.ToLowerInvariant() != e.OldFullPath.ToLowerInvariant())
                {
                    changePool.Add(e.FullPath);
                }
            }
        }

        // runs on a separate thread (created in constructor)
        private static void ChangeNotifier()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(100);
                lock (changePool)
                {
                    if (changePool.Any())
                    {
                        IList<string> pathsChanged = new List<string>(changePool);
                        FileSystemChangedInfo changedInfo = new FileSystemChangedInfo(pathsChanged);
                        FileSystemChanged(null, changedInfo);
                        changePool.Clear();
                    }
                }
            }
        }
    }
}
