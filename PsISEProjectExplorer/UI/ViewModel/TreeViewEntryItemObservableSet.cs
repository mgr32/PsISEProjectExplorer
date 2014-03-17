using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.UI.ViewModel
{
    public class TreeViewEntryItemObservableSet : INotifyCollectionChanged, IEnumerable<TreeViewEntryItemModel>
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private List<TreeViewEntryItemModel> Items { get; set; }

        public TreeViewEntryItemObservableSet()
        {
            this.Items = new List<TreeViewEntryItemModel>();
        }

        public bool Add(TreeViewEntryItemModel item)
        {
            int result = this.Items.BinarySearch(item, DefaultTreeViewEntryItemComparer.TREEVIEWENTRYITEM_COMPARER);
            if (result >= 0)
            {
                // already on list
                return false;
            }
            int index = ~result;
            this.Items.Insert(index, item);
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
            return true;
        }

        public void Clear()
        {
            this.Items.Clear();
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool Remove(TreeViewEntryItemModel item)
        {
            int index = this.Items.BinarySearch(item, DefaultTreeViewEntryItemComparer.TREEVIEWENTRYITEM_COMPARER);
            if (index < 0)
            {
                // not in list;
                return false;
            }
            this.Items.RemoveAt(index);
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
            return true;
        }

        public IEnumerator<TreeViewEntryItemModel> GetEnumerator()
        {
            return this.Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Items.GetEnumerator();
        }

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (this.CollectionChanged != null)
                this.CollectionChanged(this, e);
        }

        
    }
}
