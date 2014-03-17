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
        public static event FileSystemEventHandler FileSystemChanged;

        public static event RenamedEventHandler FileSystemRenamed;

        private static FileSystemWatcher watcher = new FileSystemWatcher();

        public static void Watch(string path)
        {
            watcher.EnableRaisingEvents = false;
            if (String.IsNullOrEmpty(path))
            {
                return;
            }
            watcher.Path = path;
            watcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size;
            watcher.IncludeSubdirectories = true;
            watcher.Changed += new FileSystemEventHandler(OnFileChanged);
            watcher.Created += new FileSystemEventHandler(OnFileChanged);
            watcher.Deleted += new FileSystemEventHandler(OnFileChanged);
            watcher.Renamed += new RenamedEventHandler(OnFileRenamed);
            watcher.EnableRaisingEvents = true;
        }

        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            FileSystemChanged(source, e);
        }

        private static void OnFileRenamed(object source, RenamedEventArgs e)
        {
            FileSystemRenamed(source, e);
        }
    }
}
