using PsISEProjectExplorer.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PsISEProjectExplorer.Model.DocHierarchy.Nodes
{
    public class DirectoryNode : AbstractNode
    {
        public override NodeType NodeType { get { return NodeType.Directory; } }

        private ISet<string> filesWithErrors = new HashSet<string>();

        private string metadata;

        public override string Metadata
        {
            get
            {
                var filesErrors = String.Join(Environment.NewLine, filesWithErrors.Select(file => "Error(s) in file " + file));
                return (String.IsNullOrEmpty(this.metadata) ? String.Empty : this.metadata + Environment.NewLine) + filesErrors;
            }
            protected set
            {
                this.metadata = value;
            }
        }

        public DirectoryNode(string path, string name, INode parent, bool isExcluded, string errorMessage)
            : base(path, name, parent, isExcluded, errorMessage == null, errorMessage)
        {
        }

        public void AddFileError(string fileName)
        {
            this.IsValid = false;
            this.filesWithErrors.Add(fileName);
        }

        public void RemoveFileError(string fileName)
        {
            this.filesWithErrors.Remove(fileName);
            this.IsValid = !this.filesWithErrors.Any();
        }
    }
}
