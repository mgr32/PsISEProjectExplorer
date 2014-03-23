using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.Model
{
    public class SearchResult
    {
        public string Path { get; private set; }

        public INode Node { get; set; }

        public SearchResult(string path)
        {
            this.Path = path;
        }
    }
}
