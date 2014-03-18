using PsISEProjectExplorer.EnumsAndOptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.Model.DocHierarchy.Nodes
{
    public interface INode
    {
        NodeType NodeType { get; }
        string Path { get; }
        string Name { get;  }
        INode Parent { get; }
        ISet<INode> Children { get; }
        bool IsExpanded { get; set; }

        // less = will be before other nodes
        int OrderValue { get; }

        void Remove();

    }
}
