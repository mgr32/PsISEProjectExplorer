using PsISEProjectExplorer.Enums;

namespace PsISEProjectExplorer.Model.DocHierarchy.Nodes
{
    public class DirectoryNode : AbstractNode
    {
        public override NodeType NodeType { get { return NodeType.Directory; } }

        public override int OrderValue { get { return -1; } }

        public DirectoryNode(string path, string name, INode parent)
            : base(path, name, parent)
        {
        }
    }
}
