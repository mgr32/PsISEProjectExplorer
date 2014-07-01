using PsISEProjectExplorer.Enums;

namespace PsISEProjectExplorer.Model
{
    public class SearchOptions
    {
        public FullTextFieldType SearchField { get; set; }

        public string SearchText { get; set; }

        public SearchOptions(FullTextFieldType searchField, string searchText)
        {
            this.SearchField = searchField;
            this.SearchText = searchText == null ? string.Empty : searchText;
        }

        public SearchOptions(SearchOptions searchOptions) : this(searchOptions.SearchField, searchOptions.SearchText)
        {
        }

        public override bool Equals(object obj)
        {
            if (!(obj is SearchOptions))
            {
                return false;
            }
            var other = (SearchOptions)obj;
            return (other.SearchField == this.SearchField && other.SearchText == this.SearchText);
        }

        public override int GetHashCode()
        {
            return (this.SearchField + (this.SearchText ?? "")).GetHashCode();
        }

    }
}
