using System;

namespace PsISEProjectExplorer.UI.Workers
{
    public class IndexerResult
    {
        public DateTime StartTimestamp { get; private set; }

        public bool IsChanged { get; private set; }

        public IndexerResult(DateTime startTimestamp, bool isChanged)
        {
            this.StartTimestamp = startTimestamp;
            this.IsChanged = isChanged;
        }
    }
}
