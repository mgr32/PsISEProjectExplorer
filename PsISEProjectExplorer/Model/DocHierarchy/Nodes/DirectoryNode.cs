using PsISEProjectExplorer.Enums;

namespace PsISEProjectExplorer.Model.DocHierarchy.Nodes
{
    public class DirectoryNode : AbstractNode
    {
        public override NodeType NodeType { get { return NodeType.Directory; } }

        public DirectoryNode(string path, string name, INode parent, string errorMessage)
            : base(path, name, parent, errorMessage == null, errorMessage)
        {
        }
    }
}
