using PsISEProjectExplorer.Model;
using PsISEProjectExplorer.Services;

namespace PsISEProjectExplorer.UI.Workers
{
    public class BackgroundSearcherParams
    {
        public DocumentHierarchySearcher DocumentHierarchySearcher { get; private set; }
        public SearchOptions SearchOptions { get; private set; }
        public string SearchText { get; private set; }
        public string Path { get; private set; }

        public BackgroundSearcherParams(DocumentHierarchySearcher documentHierarchySearcher, SearchOptions searchOptions, string searchText, string path)
        {
            this.DocumentHierarchySearcher = documentHierarchySearcher;
            this.SearchOptions = searchOptions;
            this.SearchText = searchText;
            this.Path = path;
        }
        
    }
}
