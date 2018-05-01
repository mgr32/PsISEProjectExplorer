using PsISEProjectExplorer.Enums;

namespace PsISEProjectExplorer.Model.DocHierarchy.Nodes
{
    public class RootNode : AbstractNode
    {
        public override NodeType NodeType { get { return NodeType.Intermediate; } }

        public RootNode(string path)
            : base(path, string.Empty, null, null)
        {
        }
    }
}
