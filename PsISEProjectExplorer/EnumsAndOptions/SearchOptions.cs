using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.EnumsAndOptions
{
    public class SearchOptions
    {
        public bool IncludeAllParents { get; set; }

        public FullTextFieldType SearchField { get; set; }
    }
}
