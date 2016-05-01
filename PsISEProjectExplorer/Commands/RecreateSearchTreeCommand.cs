using PsISEProjectExplorer.Services;
using PsISEProjectExplorer.UI.ViewModel;

namespace PsISEProjectExplorer.Commands
{
    [Component]
    public class RecreateSearchTreeCommand : Command
    {
        private readonly ReindexSearchTreeCommand reindexSearchTreeCommand;

        private readonly DocumentHierarchyFactory documentHierarchyFactory;

        private readonly MainViewModel mainViewModel;

        private readonly WorkspaceDirectoryModel workspaceDirectoryModel;

        public RecreateSearchTreeCommand(ReindexSearchTreeCommand reindexSearchTreeCommand, DocumentHierarchyFactory documentHierarchyFactory,
            MainViewModel mainViewModel, WorkspaceDirectoryModel workspaceDirectoryModel)
        {
            this.reindexSearchTreeCommand = reindexSearchTreeCommand;
            this.documentHierarchyFactory = documentHierarchyFactory;
            this.mainViewModel = mainViewModel;
            this.workspaceDirectoryModel = workspaceDirectoryModel;
        }

        public void Execute()
        {
            this.documentHierarchyFactory.CreateDocumentHierarchy(this.workspaceDirectoryModel.CurrentWorkspaceDirectory, this.mainViewModel.AnalyzeDocumentContents);
            this.reindexSearchTreeCommand.Execute(null);
        }

    }
}
