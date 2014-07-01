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
            this.DocumentHierarchySearcher = documentHierarchySearcher;
            this.SearchOptions = new SearchOptions(searchOptions);
            this.Path = path;
        }
        
    }
}
