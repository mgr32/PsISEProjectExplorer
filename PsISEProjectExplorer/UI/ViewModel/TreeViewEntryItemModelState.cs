namespace PsISEProjectExplorer.UI.ViewModel
{
    public class TreeViewEntryItemModelState
    {
        public bool IsExpanded { get; set; }

        public bool IsSelected { get; set; }

        public TreeViewEntryItemModelState(bool isExpanded, bool isSelected)
        {
            this.IsExpanded = isExpanded;
            this.IsSelected = isSelected;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }
            var item = (TreeViewEntryItemModelState)obj;
            return (this.IsExpanded == item.IsExpanded && this.IsSelected == item.IsSelected);
        }

        public override int GetHashCode()
        {
            return this.IsExpanded.GetHashCode() + this.IsSelected.GetHashCode();
        }
    }
}
