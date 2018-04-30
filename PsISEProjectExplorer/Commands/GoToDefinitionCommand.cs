using PsISEProjectExplorer.Model;
using PsISEProjectExplorer.Model.DocHierarchy;
using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using PsISEProjectExplorer.Services;
using PsISEProjectExplorer.UI.IseIntegration;

namespace PsISEProjectExplorer.Commands
{
    [Component]
    public class GoToDefinitionCommand : Command
    {
        private readonly IseIntegrator iseIntegrator;

        private readonly PowershellTokenizerProvider powershellTokenizerProvider;

        private readonly DocumentHierarchyFactory documentHierarchyFactory;

        private readonly DocumentHierarchySearcher documentHierarchySearcher;

        public GoToDefinitionCommand(IseIntegrator iseIntegrator, PowershellTokenizerProvider powershellTokenizerProvider, 
            DocumentHierarchyFactory documentHierarchyFactory, DocumentHierarchySearcher documentHierarchySearcher)
        {
            this.iseIntegrator = iseIntegrator;
            this.powershellTokenizerProvider = powershellTokenizerProvider;
            this.documentHierarchyFactory = documentHierarchyFactory;
            this.documentHierarchySearcher = documentHierarchySearcher;
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
            string selectedFilePath = iseIntegrator.SelectedFilePath;
            var node = (PowershellItemNode)this.documentHierarchySearcher.GetFunctionNodeByName(documentHierarchy, funcName, selectedFilePath);
            if (node == null)
            {
                return;
            }
            this.iseIntegrator.GoToFile(node.FilePath);
            this.iseIntegrator.SetCursor(node.PowershellItem.StartLine, node.PowershellItem.StartColumn);
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
