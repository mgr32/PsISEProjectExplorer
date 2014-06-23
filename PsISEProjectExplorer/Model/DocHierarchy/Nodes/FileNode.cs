using PsISEProjectExplorer.Enums;

namespace PsISEProjectExplorer.Model.DocHierarchy.Nodes
{
    public class FileNode : AbstractNode
    {
        public override NodeType NodeType { get { return NodeType.File; } }

        public FileNode(string path, string name, INode parent, string errorMessage)
            : base(path, name, parent, errorMessage == null, errorMessage)
        {
        }

    }
}
