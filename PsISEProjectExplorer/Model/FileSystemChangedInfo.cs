using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
