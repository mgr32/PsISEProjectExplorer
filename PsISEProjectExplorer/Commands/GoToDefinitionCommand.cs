using PsISEProjectExplorer.Model;
using PsISEProjectExplorer.Model.DocHierarchy;
using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using PsISEProjectExplorer.Services;
using PsISEProjectExplorer.UI.IseIntegration;

namespace PsISEProjectExplorer.Commands
{
    public class GoToDefinitionCommand : Command
    {
        private IseIntegrator IseIntegrator { get; set; }

        private PowershellTokenizerProvider PowershellTokenizerProvider { get; set; }

        private DocumentHierarchyFactory DocumentHierarchyFactory { get; set; }

        private DocumentHierarchySearcher DocumentHierarchySearcher { get; set; }

        public GoToDefinitionCommand(IseIntegrator iseIntegrator, PowershellTokenizerProvider powershellTokenizerProvider, 
            DocumentHierarchyFactory documentHierarchyFactory, DocumentHierarchySearcher documentHierarchySearcher)
        {
            this.IseIntegrator = iseIntegrator;
            this.PowershellTokenizerProvider = powershellTokenizerProvider;
            this.DocumentHierarchyFactory = documentHierarchyFactory;
            this.DocumentHierarchySearcher = documentHierarchySearcher;
        }

        public void Execute()
        {
            DocumentHierarchy documentHierarchy = this.DocumentHierarchyFactory.DocumentHierarchy;
            if (documentHierarchy == null)
            {
                return;
            }
            string funcName = this.GetFunctionNameAtCurrentPosition();
            if (funcName == null)
            {
                return;
            }
            var node = (PowershellItemNode)this.DocumentHierarchySearcher.GetFunctionNodeByName(documentHierarchy, funcName);
            if (node == null)
            {
                return;
            }
            this.IseIntegrator.GoToFile(node.FilePath);
            this.IseIntegrator.SetCursor(node.PowershellItem.StartLine, node.PowershellItem.StartColumn);
        }

        private string GetFunctionNameAtCurrentPosition()
        {
            EditorInfo editorInfo = this.IseIntegrator.GetCurrentLineWithColumnIndex();
            if (editorInfo == null)
            {
                return null;
            }
            return this.PowershellTokenizerProvider.PowershellTokenizer.GetTokenAtColumn(editorInfo.CurrentLine, editorInfo.CurrentColumn);
        }
    }
}
