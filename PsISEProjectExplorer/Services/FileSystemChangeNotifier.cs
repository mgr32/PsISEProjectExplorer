using System.Threading;
using PsISEProjectExplorer.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NLog;

namespace PsISEProjectExplorer.Services
{
    public class FileSystemChangeNotifier
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public event EventHandler<FileSystemChangedInfo> FileSystemChanged;
        
        private readonly ISet<ChangePoolEntry> ChangePool = new HashSet<ChangePoolEntry>();

        private readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        private string Name;

        public FileSystemChangeNotifier(string name)
        {
            this.Name = name;
            Task.Factory.StartNew(ChangeNotifier);
        }

        public void AddChangePoolEntry(ChangePoolEntry changePoolEntry)
        {
            lock (this)
            {
                this.ChangePool.Add(changePoolEntry);
            }
        }

        public void ClearChangePool()
        {
            lock (this)
            {
                this.ChangePool.Clear();
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
                if (CancellationTokenSource.Token.IsCancellationRequested)
                {
                    return;
                }
                FileSystemChangedInfo changedInfo = null;
                lock (this)
                {
                    if (this.ChangePool.Any())
                    {
                        IList<ChangePoolEntry> pathsChanged = RemoveSubdirectories(ChangePool);
                        changedInfo = new FileSystemChangedInfo(pathsChanged);
                        this.ChangePool.Clear();
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
                if (dirList.Select(d => d.PathChanged).Where(d => d != dir).All(d => !FileSystemOperationsService.IsSubdirectory(d, dir)))
                {
                    result.Add(entry);
                }
            }
            return result;
        }
    }

}
