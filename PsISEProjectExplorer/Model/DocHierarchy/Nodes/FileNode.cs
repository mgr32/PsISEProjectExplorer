using PsISEProjectExplorer.Enums;
using System;

namespace PsISEProjectExplorer.Model.DocHierarchy.Nodes
{
    public class FileNode : AbstractNode
    {
        public override NodeType NodeType { get { return NodeType.File; } }

        public FileNode(string path, string name, INode parent, bool isExcluded, string errorMessage)
            : base(path, name, parent, isExcluded, errorMessage == null, errorMessage)
        {
        }

        public void MakeInvalid(string errorMessage)
        {
            this.IsValid = false;
            if (this.Metadata == null)
            {
                this.Metadata = errorMessage;
            }
            else
            {
                this.Metadata += Environment.NewLine + errorMessage;
            }
        }

    }
}
