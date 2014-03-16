using ProjectExplorer.DocHierarchy;
using ProjectExplorer.DocHierarchy.HierarchyLogic;
using ProjectExplorer.EnumsAndOptions;
using ProjectExplorer.TreeView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectExplorer
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
