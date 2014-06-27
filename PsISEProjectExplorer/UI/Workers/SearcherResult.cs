using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using System;

namespace PsISEProjectExplorer.UI.Workers
{
    public class SearcherResult
    {
        public DateTime StartTimestamp { get; private set; }

        public INode ResultNode { get; private set; }

        public SearcherResult(DateTime startTimeStamp, INode resultNode)
        {
            this.StartTimestamp = startTimeStamp;
            this.ResultNode = resultNode;
        }
    }
}
