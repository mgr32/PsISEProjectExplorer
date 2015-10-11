using PsISEProjectExplorer.Enums;

namespace PsISEProjectExplorer.Model
{
    public class SearchOptions
    {
        public FullTextFieldType SearchField { get; set; }

        public string SearchText { get; set; }

        public bool SearchRegex { get; set; }

        public SearchOptions(FullTextFieldType searchField, string searchText, bool searchRegex)
        {
            this.SearchField = searchField;
            this.SearchText = searchText == null ? string.Empty : searchText;
            this.SearchRegex = searchRegex;
        }

        public SearchOptions(SearchOptions searchOptions) : this(searchOptions.SearchField, searchOptions.SearchText, searchOptions.SearchRegex)
        {
        }

        public override bool Equals(object obj)
        {
            if (!(obj is SearchOptions))
            {
                return false;
            }
            var other = (SearchOptions)obj;
            return (other.SearchField == this.SearchField && other.SearchText == this.SearchText && other.SearchRegex == this.SearchRegex);
        }

        public override int GetHashCode()
        {
            return (this.SearchField + (this.SearchText ?? "") + this.SearchRegex).GetHashCode();
        }

    }
}
