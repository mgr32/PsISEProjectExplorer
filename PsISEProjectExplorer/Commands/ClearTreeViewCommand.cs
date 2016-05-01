
using PsISEProjectExplorer.Services;
using PsISEProjectExplorer.UI.ViewModel;

namespace PsISEProjectExplorer.Commands
{
    [Component]
    public class ClearTreeViewCommand : Command
    {
        private readonly DocumentHierarchyFactory documentHierarchyFactory;

        private readonly TreeViewModel treeViewModel;

        private readonly FileSystemChangeWatcher fileSystemChangeWatcher;

        public ClearTreeViewCommand(DocumentHierarchyFactory documentHierarchyFactory, TreeViewModel treeViewModel, FileSystemChangeWatcher fileSystemChangeWatcher)
        {
            this.documentHierarchyFactory = documentHierarchyFactory;
            this.treeViewModel = treeViewModel;
            this.fileSystemChangeWatcher = fileSystemChangeWatcher;
        }

        public void Execute()
        {
            var documentHierarchy = this.documentHierarchyFactory.DocumentHierarchy;
            var rootNode = documentHierarchy == null ? null : documentHierarchy.RootNode;
            this.treeViewModel.ReRoot(rootNode);
            this.fileSystemChangeWatcher.StopWatching();
            if (rootNode != null)
            {
                this.fileSystemChangeWatcher.Watch(rootNode.Path);
            }
        }
    }
}
