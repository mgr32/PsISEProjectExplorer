using PsISEProjectExplorer.Model;
using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using System;

namespace PsISEProjectExplorer.UI.Workers
{
    public class SearcherResult
    {
        public DateTime StartTimestamp { get; private set; }

        public INode ResultNode { get; private set; }

        public string Path { get; private set; }

        public SearchOptions SearchOptions { get; private set; }

        public SearcherResult(DateTime startTimeStamp, INode resultNode, string path, SearchOptions searchOptions)
        {
			StartTimestamp = startTimeStamp;
			ResultNode = resultNode;
			Path = path;
			SearchOptions = searchOptions;
        }
    }
}
