using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace PsISEProjectExplorer.UI.ViewModel
{
	public class TreeViewEntryItemObservableSet : INotifyCollectionChanged, IEnumerable<TreeViewEntryItemModel>
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private List<TreeViewEntryItemModel> Items { get; set; }

        public TreeViewEntryItemObservableSet()
        {
			Items = new List<TreeViewEntryItemModel>();
        }

        public bool Add(TreeViewEntryItemModel item)
        {
            int indexOfAddedItem = AddItem(item);
            if (indexOfAddedItem < 0)
            {
                return false;
            }
			RaiseOnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, indexOfAddedItem));
            return true;
        }

        public void Clear()
        {
			Items.Clear();
			RaiseOnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool Remove(TreeViewEntryItemModel item)
        {
            int index = Items.BinarySearch(item, DefaultTreeViewEntryItemComparer.TreeViewEntryItemComparer);
            if (index < 0)
            {
                // not in list;
                return false;
            }
			Items.RemoveAt(index);
			RaiseOnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
            return true;
        }

        public IEnumerator<TreeViewEntryItemModel> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        private void RaiseOnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null)
				CollectionChanged(this, e);
        }

        private int AddItem(TreeViewEntryItemModel item)
        {
            int result = Items.BinarySearch(item, DefaultTreeViewEntryItemComparer.TreeViewEntryItemComparer);
            if (result >= 0)
            {
                // already on list
                return -1;
            }
            int index = ~result;
			Items.Insert(index, item);
            return index;
        }
        
    }
}
