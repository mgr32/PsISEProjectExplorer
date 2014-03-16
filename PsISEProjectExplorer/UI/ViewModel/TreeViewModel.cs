using PsISEProjectExplorer.EnumsAndOptions;
using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using PsISEProjectExplorer.UI.IseIntegration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.UI.ViewModel
{
    public class TreeViewModel : BaseViewModel
    {
        private TreeViewEntryItem rootTreeViewEntryItem;

        public TreeViewEntryItem RootTreeViewEntryItem
        {
            get { return this.rootTreeViewEntryItem; }
            set
            {
                this.rootTreeViewEntryItem = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged("TreeViewItems");
            }
        }

        public IList<TreeViewEntryItem> TreeViewItems
        {
            get
            {
                if (this.RootTreeViewEntryItem == null)
                {
                    return new List<TreeViewEntryItem>();
                }
                return this.RootTreeViewEntryItem.Children;
            }
        }

        public IseIntegrator IseIntegrator { get; set; }

        public void SelectItem(TreeViewEntryItem item)
        {
            if (this.IseIntegrator == null)
            {
                throw new InvalidOperationException("IseIntegrator has not ben set yet.");
            }
            if (item != null)
            {
                if (item.Node.NodeType == NodeType.FILE)
                {
                    this.IseIntegrator.GoToFile(item.Node.Path);
                }
                else if (item.Node.NodeType == NodeType.FUNCTION)
                {
                    PowershellFunctionNode node = ((PowershellFunctionNode)item.Node);
                    this.IseIntegrator.GoToFile(node.FilePath);
                    this.IseIntegrator.SetCursor(node.PowershellFunction.StartLine, node.PowershellFunction.StartColumn);
                }
            }
        }

    }
}
