using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectExplorer.DocHierarchy.Nodes
{
    public class FileNode : FileSystemNode
    {
        public FileNode(string path, string name, INode parent)
            : base(path, name, parent)
        {
        }
    }
}
