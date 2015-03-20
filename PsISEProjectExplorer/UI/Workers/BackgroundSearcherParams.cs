using PsISEProjectExplorer.Model;
using PsISEProjectExplorer.Services;

namespace PsISEProjectExplorer.UI.Workers
{
    public class BackgroundSearcherParams
    {
        public DocumentHierarchySearcher DocumentHierarchySearcher { get; private set; }
        public SearchOptions SearchOptions { get; private set; }
        public string Path { get; private set; }

        public BackgroundSearcherParams(DocumentHierarchySearcher documentHierarchySearcher, SearchOptions searchOptions, string path)
        {
			DocumentHierarchySearcher = documentHierarchySearcher;
			SearchOptions = new SearchOptions(searchOptions);
			Path = path;
        }
        
    }
}
