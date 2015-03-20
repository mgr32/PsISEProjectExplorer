using PsISEProjectExplorer.Services;
using System.Collections.Generic;

namespace PsISEProjectExplorer.UI.Workers
{
	public class BackgroundIndexerParams
    {
        public DocumentHierarchyFactory DocumentHierarchyFactory { get; private set; }
        public string RootDirectory { get; private set; }
        public IEnumerable<string> PathsChanged { get; private set; }
        public FilesPatternProvider FilesPatternProvider { get; private set; }

        public BackgroundIndexerParams(DocumentHierarchyFactory documentHierarchyFactory, string rootDirectory, IEnumerable<string> pathsChanged, FilesPatternProvider filesPatternProvider)
        {
			DocumentHierarchyFactory = documentHierarchyFactory;
			RootDirectory = rootDirectory;
			PathsChanged = pathsChanged;
			FilesPatternProvider = filesPatternProvider;
        }
        
    }
}
