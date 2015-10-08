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

        private FileSystemChangeNotifier FileSystemChangeNotifier { get; set; }

        private FileSystemWatcher Watcher { get; set; }

        public ISEFile IseFile { get; private set; }

        public IseFileWatcher(FileSystemChangeNotifier fileSystemChangeNotifier, string path, ISEFile iseFile)
        {
            this.IseFile = iseFile;
            this.FileSystemChangeNotifier = fileSystemChangeNotifier;
            this.Watcher = new FileSystemWatcher(Path.GetDirectoryName(path), Path.GetFileName(path));
            this.Watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.Security;
            this.Watcher.Changed += OnFileChanged;
            this.Watcher.Deleted += OnFileChanged;
            this.Watcher.Renamed += OnFileRenamed;
            this.Watcher.EnableRaisingEvents = true;
        }

        public void StopWatching()
        {
            this.Watcher.EnableRaisingEvents = false;
            this.IseFile = null;
        }

        private void OnFileChanged(object source, FileSystemEventArgs e)
        {
            Logger.Debug("File changed: " + e.FullPath);
            this.FileSystemChangeNotifier.AddChangePoolEntry(new ChangePoolEntry(e.FullPath, String.Empty));
        }

        private void OnFileRenamed(object source, RenamedEventArgs e)
        {
            Logger.Debug("File renamed: " + e.OldFullPath + " to " + e.FullPath);
            this.FileSystemChangeNotifier.AddChangePoolEntry(new ChangePoolEntry(e.OldFullPath, String.Empty, e.FullPath));
        }
    }
}
