using PsISEProjectExplorer.Enums;

namespace PsISEProjectExplorer.Model
{
    public class SearchOptions
    {
        public bool IncludeAllParents { get; set; }

        public FullTextFieldType SearchField { get; set; }
    }
}
