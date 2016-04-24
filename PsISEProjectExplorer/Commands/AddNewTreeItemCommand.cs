using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using PsISEProjectExplorer.Services;
using PsISEProjectExplorer.UI.ViewModel;

namespace PsISEProjectExplorer.Commands
{
    public class AddNewTreeItemCommand : ParameterizedCommand<NodeType>
    {
        private TreeViewModel TreeViewModel { get; set; }

        private DocumentHierarchyFactory DocumentHierarchyFactory { get; set; }

        public AddNewTreeItemCommand(TreeViewModel treeViewModel, DocumentHierarchyFactory documentHierarchyFactory)
        {
            this.TreeViewModel = treeViewModel;
            this.DocumentHierarchyFactory = documentHierarchyFactory;
        }

        public void Execute(NodeType nodeType)
        {
            var parent = this.TreeViewModel.SelectedItem;
            if (this.DocumentHierarchyFactory == null)
            {
                return;
            }
            if (parent == null)
            {
                parent = this.TreeViewModel.RootTreeViewEntryItem;
            }
            parent.IsExpanded = true;
            INode newNode = this.DocumentHierarchyFactory.CreateTemporaryNode(parent.Node, nodeType);
            if (newNode == null)
            {
                return;
            }
            var newItem = this.TreeViewModel.CreateTreeViewEntryItemModel(newNode, parent, true);
            newItem.IsBeingEdited = true;
            newItem.IsBeingAdded = true;
        }
    }
}
