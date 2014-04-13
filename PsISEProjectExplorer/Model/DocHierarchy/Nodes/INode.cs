using PsISEProjectExplorer.Enums;
using System.Collections.Generic;

namespace PsISEProjectExplorer.Model.DocHierarchy.Nodes
{
    public interface INode
    {
        NodeType NodeType { get; }
        string Path { get; }
        string Name { get;  }
        INode Parent { get; }
        ISet<INode> Children { get; }

        void Remove();
    }
}
