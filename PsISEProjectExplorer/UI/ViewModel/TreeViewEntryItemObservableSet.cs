using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace PsISEProjectExplorer.UI.ViewModel
{
    public class TreeViewEntryItemObservableSet : INotifyCollectionChanged, IEnumerable<TreeViewEntryItemModel>
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private readonly List<TreeViewEntryItemModel> items;

        public TreeViewEntryItemObservableSet()
        {
            this.items = new List<TreeViewEntryItemModel>();
        }

        public bool Add(TreeViewEntryItemModel item)
        {
            int indexOfAddedItem = this.AddItem(item);
            if (indexOfAddedItem < 0)
            {
                return false;
            }
            this.RaiseOnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, indexOfAddedItem));
            return true;
        }

        public void Clear()
        {
            this.items.Clear();
            this.RaiseOnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool Remove(TreeViewEntryItemModel item)
        {
            int index = this.items.BinarySearch(item, DefaultTreeViewEntryItemComparer.TreeViewEntryItemComparer);
            if (index < 0)
            {
                // not in list;
                return false;
            }
            this.items.RemoveAt(index);
            this.RaiseOnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
            return true;
        }

        public IEnumerator<TreeViewEntryItemModel> GetEnumerator()
        {
            return this.items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.items.GetEnumerator();
        }

        private void RaiseOnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (this.CollectionChanged != null)
                this.CollectionChanged(this, e);
        }

        private int AddItem(TreeViewEntryItemModel item)
        {
            int result = this.items.BinarySearch(item, DefaultTreeViewEntryItemComparer.TreeViewEntryItemComparer);
            if (result >= 0)
            {
                // already on list
                return -1;
            }
            int index = ~result;
            this.items.Insert(index, item);
            return index;
        }
        
    }
}
