using PsISEProjectExplorer.Config;
using PsISEProjectExplorer.Services;
using PsISEProjectExplorer.UI.ViewModel;

namespace PsISEProjectExplorer.Commands
{
    public class ExcludeOrIncludeItemCommand : Command
    {

        private MainViewModel MainViewModel { get; set; }

        private TreeViewModel TreeViewModel { get; set; }

        private FilesPatternProvider FilesPatternProvider { get; set; }

        private ConfigHandler ConfigHandler { get; set; }

        public ExcludeOrIncludeItemCommand(MainViewModel mainViewModel, TreeViewModel treeViewModel, FilesPatternProvider filesPatternProvider, ConfigHandler configHandler)
        {
            this.MainViewModel = mainViewModel;
            this.TreeViewModel = treeViewModel;
            this.FilesPatternProvider = filesPatternProvider;
            this.ConfigHandler = configHandler;
        }

        public void Execute()
        {
            var selectedItem = this.TreeViewModel.SelectedItem;
            if (selectedItem == null || selectedItem.IsBeingEdited)
            {
                return;
            }

            if (selectedItem.IsExcluded)
            {
                this.FilesPatternProvider.ExcludePaths = this.ConfigHandler.RemoveConfigEnumerableValue("ExcludePaths", selectedItem.Path);
            }
            else
            {
                this.FilesPatternProvider.ExcludePaths = this.ConfigHandler.AddConfigEnumerableValue("ExcludePaths", selectedItem.Path);
            }
            this.MainViewModel.ReindexSearchTree();
        }
    }
}
