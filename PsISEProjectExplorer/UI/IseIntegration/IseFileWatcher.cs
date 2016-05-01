using Microsoft.PowerShell.Host.ISE;
using NLog;
using PsISEProjectExplorer.Model;
using PsISEProjectExplorer.Services;
using System;
using System.IO;

namespace PsISEProjectExplorer.UI.IseIntegration
{
    public class IseFileWatcher
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly FileSystemChangeNotifier fileSystemChangeNotifier;

        private readonly FileSystemWatcher watcher;

        private ISEFile iseFile;

        public IseFileWatcher(FileSystemChangeNotifier fileSystemChangeNotifier, string path, ISEFile iseFile)
        {
            this.iseFile = iseFile;
            this.fileSystemChangeNotifier = fileSystemChangeNotifier;
            this.watcher = new FileSystemWatcher(Path.GetDirectoryName(path), Path.GetFileName(path));
            this.watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.Security;
            this.watcher.Changed += OnFileChanged;
            this.watcher.Deleted += OnFileChanged;
            this.watcher.Renamed += OnFileRenamed;
            this.watcher.EnableRaisingEvents = true;
        }

        public void StopWatching()
        {
            this.watcher.EnableRaisingEvents = false;
            this.iseFile = null;
        }

        private void OnFileChanged(object source, FileSystemEventArgs e)
        {
            Logger.Debug("File changed: " + e.FullPath);
            this.fileSystemChangeNotifier.AddChangePoolEntry(new ChangePoolEntry(e.FullPath, String.Empty));
        }

        private void OnFileRenamed(object source, RenamedEventArgs e)
        {
            Logger.Debug("File renamed: " + e.OldFullPath + " to " + e.FullPath);
            this.fileSystemChangeNotifier.AddChangePoolEntry(new ChangePoolEntry(e.OldFullPath, String.Empty, e.FullPath));
        }
    }
}
