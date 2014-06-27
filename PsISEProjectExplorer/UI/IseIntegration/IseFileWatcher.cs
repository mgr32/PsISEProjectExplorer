using PsISEProjectExplorer.Model;
using PsISEProjectExplorer.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.UI.IseIntegration
{
    public class IseFileWatcher
    {
        private FileSystemChangeNotifier FileSystemChangeNotifier { get; set; }

        private FileSystemWatcher Watcher { get; set; }

        public IseFileWatcher(FileSystemChangeNotifier fileSystemChangeNotifier, string path)
        {
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
        }

        public void StartWatching()
        {
            this.Watcher.EnableRaisingEvents = true;
        }

        private void OnFileChanged(object source, FileSystemEventArgs e)
        {
            this.FileSystemChangeNotifier.AddChangePoolEntry(new ChangePoolEntry(e.FullPath, String.Empty));
        }

        private void OnFileRenamed(object source, RenamedEventArgs e)
        {
            this.FileSystemChangeNotifier.AddChangePoolEntry(new ChangePoolEntry(e.OldFullPath, String.Empty, e.FullPath));
        }

    }
}
