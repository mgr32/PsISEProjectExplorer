using PsISEProjectExplorer.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.Model.DocHierarchy.Nodes
{
    public class FileNode : AbstractNode
    {
        public override NodeType NodeType { get { return NodeType.FILE; } }

        public FileNode(string path, string name, INode parent)
            : base(path, name, parent)
        {
        }

    }
}
