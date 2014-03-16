using PsISEProjectExplorer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.UI.Workers
{
    public class BackgroundIndexerParams
    {
        public DocumentHierarchyFactory DocumentHierarchyIndexer { get; private set; }
        public string RootDirectory { get; private set; }
        public IEnumerable<string> FilesChanged { get; private set; }

        public BackgroundIndexerParams(DocumentHierarchyFactory documentHierarchyIndexer, string rootDirectory, IEnumerable<string> filesChanged)
        {
            this.DocumentHierarchyIndexer = documentHierarchyIndexer;
            this.RootDirectory = rootDirectory;
            this.FilesChanged = filesChanged;
        }
        
    }
}
