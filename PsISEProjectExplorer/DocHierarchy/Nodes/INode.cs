using ProjectExplorer.EnumsAndOptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectExplorer.DocHierarchy.Nodes
{
    public interface INode
    {
        NodeType NodeType { get; }
        string Path { get; }
        string Name { get;  }
        INode Parent { get; }
        ICollection<INode> Children { get; }

        // less = will be before other nodes
        int OrderValue { get; }

    }
}
