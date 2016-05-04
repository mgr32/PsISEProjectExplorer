using PsISEProjectExplorer.Config;
using PsISEProjectExplorer.Services;
using PsISEProjectExplorer.UI.ViewModel;

namespace PsISEProjectExplorer.Commands
{
    [Component]
    public class ExcludeOrIncludeItemCommand : Command
    {
        private readonly MainViewModel mainViewModel;

        private readonly TreeViewModel treeViewModel;

        private readonly FilesPatternProvider filesPatternProvider;

        private readonly ConfigValues configValues;

        private readonly ReindexSearchTreeCommand reindexSearchTreeCommand;

        public ExcludeOrIncludeItemCommand(MainViewModel mainViewModel, TreeViewModel treeViewModel, FilesPatternProvider filesPatternProvider, ConfigValues configValues,
            ReindexSearchTreeCommand reindexSearchTreeCommand)
        {
            this.mainViewModel = mainViewModel;
            this.treeViewModel = treeViewModel;
            this.filesPatternProvider = filesPatternProvider;
            this.configValues = configValues;
            this.reindexSearchTreeCommand = reindexSearchTreeCommand;
        }

        public void Execute()
        {
            var selectedItem = this.treeViewModel.SelectedItem;
            if (selectedItem == null || selectedItem.IsBeingEdited)
            {
                return;
            }

            if (selectedItem.IsExcluded)
            {
                this.filesPatternProvider.ExcludePaths = this.configValues.RemoveExcludePath(selectedItem.Path);
            }
            else
            {
                this.filesPatternProvider.ExcludePaths = this.configValues.AddExcludePath(selectedItem.Path);
            }
            this.reindexSearchTreeCommand.Execute(null);
        }
    }
}
