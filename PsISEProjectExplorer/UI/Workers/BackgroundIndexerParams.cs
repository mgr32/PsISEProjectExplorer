using PsISEProjectExplorer.Services;
using System.Collections.Generic;

namespace PsISEProjectExplorer.UI.Workers
{
    public class BackgroundIndexerParams
    {
        public DocumentHierarchyFactory DocumentHierarchyFactory { get; private set; }
        public string RootDirectory { get; private set; }
        public IEnumerable<string> PathsChanged { get; private set; }
        public IEnumerable<string> ExcludePaths { get; private set; }
        public FilesPatternProvider FilesPatternProvider { get; private set; }

        public BackgroundIndexerParams(DocumentHierarchyFactory documentHierarchyFactory, string rootDirectory, IEnumerable<string> pathsChanged, IEnumerable<string> excludePaths, FilesPatternProvider filesPatternProvider)
        {
            this.DocumentHierarchyFactory = documentHierarchyFactory;
            this.RootDirectory = rootDirectory;
            this.PathsChanged = pathsChanged;
            this.ExcludePaths = excludePaths;
            this.FilesPatternProvider = filesPatternProvider;
        }
        
    }
}
