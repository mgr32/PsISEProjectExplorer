using PsISEProjectExplorer.EnumsAndOptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.DocHierarchy.Nodes
{
    public class RootNode : AbstractNode
    {
        public override NodeType NodeType { get { return NodeType.INTERMEDIATE; } }

        public RootNode(string path)
            : base(path, string.Empty, null)
        {
        }
    }
}
