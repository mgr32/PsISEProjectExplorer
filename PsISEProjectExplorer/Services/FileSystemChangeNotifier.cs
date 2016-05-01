using System.Threading;
using PsISEProjectExplorer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;

namespace PsISEProjectExplorer.Services
{
    public class FileSystemChangeNotifier
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public event EventHandler<FileSystemChangedInfo> FileSystemChanged;

        private readonly ISet<ChangePoolEntry> changePool = new HashSet<ChangePoolEntry>();

        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private readonly string Name;

        private readonly FileSystemOperationsService fileSystemOperationsService;

        public FileSystemChangeNotifier(string name, FileSystemOperationsService fileSystemOperationsService)
        {
            this.fileSystemOperationsService = fileSystemOperationsService;
            this.Name = name;
            Task.Factory.StartNew(ChangeNotifier);
        }

        public void AddChangePoolEntry(ChangePoolEntry changePoolEntry)
        {
            lock (this)
            {
                this.changePool.Add(changePoolEntry);
            }
        }

        public void ClearChangePool()
        {
            lock (this)
            {
                this.changePool.Clear();
            }
        }

        // runs on a separate thread (created in constructor)
        private void ChangeNotifier()
        {
            if (Thread.CurrentThread.Name == null)
            {
                Thread.CurrentThread.Name = this.Name;
            }
            while (true)
            {
                Thread.Sleep(200);
                if (cancellationTokenSource.Token.IsCancellationRequested)
                {
                    return;
                }
                FileSystemChangedInfo changedInfo = null;
                lock (this)
                {
                    if (this.changePool.Any())
                    {
                        IList<ChangePoolEntry> pathsChanged = RemoveSubdirectories(changePool);
                        changedInfo = new FileSystemChangedInfo(pathsChanged);
                        this.changePool.Clear();
                    }
                }
                if (changedInfo != null && this.FileSystemChanged != null)
                {
                    FileSystemChanged(this, changedInfo);
                }
            }
        }

        private IList<ChangePoolEntry> RemoveSubdirectories(ISet<ChangePoolEntry> dirList)
        {
            IList<ChangePoolEntry> result = new List<ChangePoolEntry>();
            foreach (ChangePoolEntry entry in dirList)
            {
                string dir = entry.PathChanged;
                if (dirList.Select(d => d.PathChanged).Where(d => d != dir).All(d => !fileSystemOperationsService.IsSubdirectory(d, dir)))
                {
                    result.Add(entry);
                }
            }
            return result;
        }
    }

}
