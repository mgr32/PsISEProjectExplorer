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

        private static FileSystemWatcher watcher = new FileSystemWatcher();

        public static void Watch(string path)
        {
            watcher.EnableRaisingEvents = false;
            if (String.IsNullOrEmpty(path))
            {
                return;
            }
            watcher.Path = path;
            watcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.Security;
            watcher.IncludeSubdirectories = true;
            watcher.Changed += new FileSystemEventHandler(OnFileChanged);
            watcher.Created += new FileSystemEventHandler(OnFileChanged);
            watcher.Deleted += new FileSystemEventHandler(OnFileChanged);
            watcher.Renamed += new RenamedEventHandler(OnFileRenamed);
            watcher.EnableRaisingEvents = true;
        }

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
            FileSystemChangedInfo changedInfo = new FileSystemChangedInfo(new List<string>() { e.FullPath });
            FileSystemChanged(source, changedInfo);
        }

        private static void OnFileRenamed(object source, RenamedEventArgs e)
        {
            IList<string> pathsChanged = new List<string>();
            bool isDir = Directory.Exists(e.FullPath) || Directory.Exists(e.OldFullPath);
            if (isDir || FilesPatternProvider.POWERSHELL_FILES_REGEX.IsMatch(e.OldFullPath))
            {
                pathsChanged.Add(e.OldFullPath);
            }
            if ((isDir || FilesPatternProvider.POWERSHELL_FILES_REGEX.IsMatch(e.FullPath)) &&
                e.FullPath.ToLowerInvariant() != e.OldFullPath.ToLowerInvariant())
            {
                pathsChanged.Add(e.FullPath);
            }
            FileSystemChangedInfo changedInfo = new FileSystemChangedInfo(new List<string>() { e.FullPath });
            FileSystemChanged(source, changedInfo);
        }
    }
}
