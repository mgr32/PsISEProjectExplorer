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
			IseFile = iseFile;
			FileSystemChangeNotifier = fileSystemChangeNotifier;
			Watcher = new FileSystemWatcher(Path.GetDirectoryName(path), Path.GetFileName(path));
			Watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.Security;
			Watcher.Changed += OnFileChanged;
			Watcher.Deleted += OnFileChanged;
			Watcher.Renamed += OnFileRenamed;
			Watcher.EnableRaisingEvents = true;
        }

        public void StopWatching()
        {
			Watcher.EnableRaisingEvents = false;
			IseFile = null;
        }

        private void OnFileChanged(object source, FileSystemEventArgs e)
        {
            Logger.Debug("File changed: " + e.FullPath);
			FileSystemChangeNotifier.AddChangePoolEntry(new ChangePoolEntry(e.FullPath, String.Empty));
        }

        private void OnFileRenamed(object source, RenamedEventArgs e)
        {
            Logger.Debug("File renamed: " + e.OldFullPath + " to " + e.FullPath);
			FileSystemChangeNotifier.AddChangePoolEntry(new ChangePoolEntry(e.OldFullPath, String.Empty, e.FullPath));
        }

    }
}
