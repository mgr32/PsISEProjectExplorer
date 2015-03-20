using PsISEProjectExplorer.Enums;

namespace PsISEProjectExplorer.Model
{
    public class SearchOptions
    {
        public FullTextFieldType SearchField { get; set; }

        public string SearchText { get; set; }

        public SearchOptions(FullTextFieldType searchField, string searchText)
        {
			SearchField = searchField;
			SearchText = searchText == null ? string.Empty : searchText;
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
            return (other.SearchField == SearchField && other.SearchText == SearchText);
        }

        public override int GetHashCode()
        {
            return (SearchField + (SearchText ?? "")).GetHashCode();
        }

    }
}
