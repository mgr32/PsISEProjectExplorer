using System.Threading;
using PsISEProjectExplorer.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.Services
{
    public class FileSystemChangeNotifier
    {
        public event EventHandler<FileSystemChangedInfo> FileSystemChanged;
        
        private readonly ISet<ChangePoolEntry> ChangePool = new HashSet<ChangePoolEntry>();

        private readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        public FileSystemChangeNotifier()
        {
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
            while (true)
            {
                Thread.Sleep(200);
                if (CancellationTokenSource.Token.IsCancellationRequested)
                {
                    return;
                }
                lock (this)
                {
                    if (this.ChangePool.Any())
                    {
                        IList<ChangePoolEntry> pathsChanged = RemoveSubdirectories(ChangePool);
                        var changedInfo = new FileSystemChangedInfo(pathsChanged);
                        if (this.FileSystemChanged != null)
                        {
                            FileSystemChanged(this, changedInfo);
                        }
                        this.ChangePool.Clear();
                    }
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
