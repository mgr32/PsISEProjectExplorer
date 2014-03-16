using PsISEProjectExplorer.DocHierarchy;
using PsISEProjectExplorer.DocHierarchy.HierarchyLogic;
using PsISEProjectExplorer.EnumsAndOptions;
using PsISEProjectExplorer.UI.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.UI.Workers
{
    public class BackgroundSearcherParams
    {
        public DocumentHierarchySearcher DocumentHierarchySearcher { get; private set; }
        public SearchOptions SearchOptions { get; private set; }
        public string SearchText { get; private set; }

        public BackgroundSearcherParams(DocumentHierarchySearcher documentHierarchySearcher, SearchOptions searchOptions, string searchText)
        {
            this.DocumentHierarchySearcher = documentHierarchySearcher;
            this.SearchOptions = searchOptions;
            this.SearchText = searchText;
        }
        
    }
}
