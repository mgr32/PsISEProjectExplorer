using PsISEProjectExplorer.Model;
using PsISEProjectExplorer.Model.DocHierarchy;
using PsISEProjectExplorer.Services;

namespace PsISEProjectExplorer.UI.Workers
{
    public class BackgroundSearcherParams
    {
        public DocumentHierarchy DocumentHierarchy { get; private set; }
        public SearchOptions SearchOptions { get; private set; }
        public string Path { get; private set; }

        public BackgroundSearcherParams(DocumentHierarchy documentHierarchy, SearchOptions searchOptions, string path)
        {
            this.DocumentHierarchy = documentHierarchy;
            this.SearchOptions = new SearchOptions(searchOptions);
            this.Path = path;
        }
        
    }
}
