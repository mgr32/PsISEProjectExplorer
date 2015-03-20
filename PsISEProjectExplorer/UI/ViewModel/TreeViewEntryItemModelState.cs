namespace PsISEProjectExplorer.UI.ViewModel
{
	public class TreeViewEntryItemModelState
    {
        public bool IsExpanded { get; set; }

        public bool IsSelected { get; set; }

        public TreeViewEntryItemModelState(bool isExpanded, bool isSelected)
        {
			IsExpanded = isExpanded;
			IsSelected = isSelected;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            var item = (TreeViewEntryItemModelState)obj;
            return (IsExpanded == item.IsExpanded && IsSelected == item.IsSelected);
        }

        public override int GetHashCode()
        {
            return IsExpanded.GetHashCode() + IsSelected.GetHashCode();
        }
    }
}
