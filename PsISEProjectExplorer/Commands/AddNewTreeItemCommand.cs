using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.UI.ViewModel;

namespace PsISEProjectExplorer.Commands
{
    public class AddNewTreeItemCommand : ParameterizedCommand<NodeType>
    {
        private TreeViewModel TreeViewModel { get; set; }

        public AddNewTreeItemCommand(TreeViewModel treeViewModel)
        {
            this.TreeViewModel = treeViewModel;
        }

        public void Execute(NodeType nodeType)
        {
            var item = this.TreeViewModel.SelectedItem;
            this.TreeViewModel.AddNewTreeItem(item, nodeType);
        }
    }
}
