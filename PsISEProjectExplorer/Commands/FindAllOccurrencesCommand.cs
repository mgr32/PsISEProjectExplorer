using PsISEProjectExplorer.Model;
using PsISEProjectExplorer.Model.DocHierarchy;
using PsISEProjectExplorer.Services;
using PsISEProjectExplorer.UI.IseIntegration;
using PsISEProjectExplorer.UI.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.Commands
{
    public class FindAllOccurrencesCommand : Command
    {
        private IseIntegrator IseIntegrator { get; set; }

        private PowershellTokenizerProvider PowershellTokenizerProvider { get; set; }

        private MainViewModel MainViewModel { get; set; }

        private DocumentHierarchyFactory DocumentHierarchyFactory { get; set; }

        public FindAllOccurrencesCommand(IseIntegrator iseIntegrator, PowershellTokenizerProvider powershellTokenizerProvider, MainViewModel mainViewModel,
            DocumentHierarchyFactory documentHierarchyFactory)
        {
            this.IseIntegrator = iseIntegrator;
            this.PowershellTokenizerProvider = powershellTokenizerProvider;
            this.MainViewModel = mainViewModel;
            this.DocumentHierarchyFactory = documentHierarchyFactory;
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

            // TODO: this is hacky...
            this.MainViewModel.SearchOptions.SearchText = string.Empty;
            this.MainViewModel.SearchInFiles = true;
            this.MainViewModel.SearchText = funcName;
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
