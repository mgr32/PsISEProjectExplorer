using System.Collections.Generic;

namespace PsISEProjectExplorer.Model
{
    public class FileSystemChangedInfo
    {
        public IEnumerable<ChangePoolEntry> PathsChanged { get; private set; }

        public FileSystemChangedInfo(IEnumerable<ChangePoolEntry> pathsChanged)
        {
			PathsChanged = pathsChanged;
        }
    }
}
