using PsISEProjectExplorer.Model.DocHierarchy.Nodes;

namespace PsISEProjectExplorer.Model
{
    public class SearchResult
    {
        public string Path { get; private set; }

        public INode Node { get; set; }

        public SearchResult(string path)
        {
            this.Path = path;
        }
    }
}
