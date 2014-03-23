using PsISEProjectExplorer.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.Model.DocHierarchy.Nodes
{
    public class DirectoryNode : AbstractNode
    {
        public override NodeType NodeType { get { return NodeType.DIRECTORY; } }

        public override int OrderValue { get { return -1; } }

        public DirectoryNode(string path, string name, INode parent)
            : base(path, name, parent)
        {
        }
    }
}
