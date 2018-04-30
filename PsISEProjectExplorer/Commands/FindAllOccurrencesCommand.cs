using PsISEProjectExplorer.Model;
using PsISEProjectExplorer.Model.DocHierarchy;
using PsISEProjectExplorer.Services;
using PsISEProjectExplorer.UI.IseIntegration;
using PsISEProjectExplorer.UI.ViewModel;

namespace PsISEProjectExplorer.Commands
{
    [Component]
    public class FindAllOccurrencesCommand : Command
    {
        private readonly IseIntegrator iseIntegrator;

        private readonly PowershellTokenizerProvider powershellTokenizerProvider;

        private readonly MainViewModel mainViewModel;

        private readonly DocumentHierarchyFactory documentHierarchyFactory;

        public FindAllOccurrencesCommand(IseIntegrator iseIntegrator, PowershellTokenizerProvider powershellTokenizerProvider, MainViewModel mainViewModel,
            DocumentHierarchyFactory documentHierarchyFactory)
        {
            this.iseIntegrator = iseIntegrator;
            this.powershellTokenizerProvider = powershellTokenizerProvider;
            this.mainViewModel = mainViewModel;
            this.documentHierarchyFactory = documentHierarchyFactory;
        }

        public void Execute()
        {
            DocumentHierarchy documentHierarchy = this.documentHierarchyFactory.DocumentHierarchy;
            if (documentHierarchy == null)
            {
                return;
            }
            string funcName = this.GetFunctionNameAtCurrentPosition();
            if (funcName == null)
            {
                return;
            }

            // TODO: this is hacky...
            this.mainViewModel.SearchOptions.SearchText = string.Empty;
            if (this.mainViewModel.IndexFilesMode == IndexingMode.NO_FILES)
            {
                this.mainViewModel.IndexFilesMode = IndexingMode.LOCAL_FILES;
            }
            this.mainViewModel.SearchText = funcName;
        }

        private string GetFunctionNameAtCurrentPosition()
        {
            EditorInfo editorInfo = this.iseIntegrator.GetCurrentLineWithColumnIndex();
            if (editorInfo == null)
            {
                return null;
            }
            return this.powershellTokenizerProvider.PowershellTokenizer.GetTokenAtColumn(editorInfo.CurrentLine, editorInfo.CurrentColumn);
        }
    }
}
