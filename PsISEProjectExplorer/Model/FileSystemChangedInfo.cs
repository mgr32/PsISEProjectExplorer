using System.Collections.Generic;

namespace PsISEProjectExplorer.Model
{
    public class FileSystemChangedInfo
    {
        public IEnumerable<string> PathsChanged { get; private set; }

        public FileSystemChangedInfo(IEnumerable<string> pathsChanged)
        {
            this.PathsChanged = pathsChanged;
        }
    }
}
