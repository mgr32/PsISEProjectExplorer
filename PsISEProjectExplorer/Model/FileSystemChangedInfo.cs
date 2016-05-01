using System.Collections.Generic;

namespace PsISEProjectExplorer.Model
{
    public class FileSystemChangedInfo
    {
        private IEnumerable<ChangePoolEntry> pathsChanged;

        public IEnumerable<ChangePoolEntry> PathsChanged { get { return this.pathsChanged; } }

        public FileSystemChangedInfo(IEnumerable<ChangePoolEntry> pathsChanged)
        {
            this.pathsChanged = pathsChanged;
        }
    }
}
