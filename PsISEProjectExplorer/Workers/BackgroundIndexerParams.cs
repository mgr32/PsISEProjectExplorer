using ProjectExplorer.DocHierarchy;
using ProjectExplorer.DocHierarchy.HierarchyLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectExplorer
{
    public class BackgroundIndexerParams
    {
        public DocumentHierarchies DocumentHierarchies { get; private set; }
        public string RootDirectory { get; private set; }
        public IEnumerable<string> FilesChanged { get; private set; }

        public BackgroundIndexerParams(DocumentHierarchies documentHierarchies, string rootDirectory, IEnumerable<string> filesChanged)
        {
            this.DocumentHierarchies = documentHierarchies;
            this.RootDirectory = rootDirectory;
            this.FilesChanged = filesChanged;
        }
        
    }
}
