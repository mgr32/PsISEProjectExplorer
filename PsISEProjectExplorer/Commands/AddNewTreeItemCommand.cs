using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using PsISEProjectExplorer.Services;
using PsISEProjectExplorer.UI.ViewModel;

namespace PsISEProjectExplorer.Commands
{
    [Component]
    public class AddNewTreeItemCommand : ParameterizedCommand<NodeType>
    {
        private readonly TreeViewModel treeViewModel;

        private readonly DocumentHierarchyFactory documentHierarchyFactory;

        public AddNewTreeItemCommand(TreeViewModel treeViewModel, DocumentHierarchyFactory documentHierarchyFactory)
        {
            this.treeViewModel = treeViewModel;
            this.documentHierarchyFactory = documentHierarchyFactory;
        }

        public void Execute(NodeType nodeType)
        {
            var parent = this.treeViewModel.SelectedItem;
            if (this.documentHierarchyFactory == null)
            {
                return;
            }
            if (parent == null)
            {
                parent = this.treeViewModel.RootTreeViewEntryItem;
            }
            parent.IsExpanded = true;
            INode newNode = this.documentHierarchyFactory.CreateTemporaryNode(parent.Node, nodeType);
            if (newNode == null)
            {
                return;
            }
            var newItem = this.treeViewModel.CreateTreeViewEntryItemModel(newNode, parent, true);
            newItem.IsBeingEdited = true;
            newItem.IsBeingAdded = true;
        }
    }
}
