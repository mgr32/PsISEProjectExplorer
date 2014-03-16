using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectExplorer.DocHierarchy.HierarchyLogic
{
    public class PowershellFunction
    {
        public string Name { get; private set; }

        public int StartLine { get; private set; }

        public int StartColumn { get; private set; }

        public PowershellFunction(string name, int startLine, int startColumn)
        {
            this.Name = name;
            this.StartLine = startLine;
            this.StartColumn = startColumn;
        }
    }
}
