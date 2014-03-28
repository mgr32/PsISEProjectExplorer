using PsISEProjectExplorer.Enums;
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

        // less = will be before other nodes
        int OrderValue { get; }

        void Remove();

    }
}
