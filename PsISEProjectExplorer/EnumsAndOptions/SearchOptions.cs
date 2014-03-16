using ProjectExplorer.DocHierarchy.FullText;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectExplorer.EnumsAndOptions
{
    public class SearchOptions
    {
        public bool IncludeAllParents { get; set; }

        public FullTextFieldType SearchField { get; set; }
    }
}
