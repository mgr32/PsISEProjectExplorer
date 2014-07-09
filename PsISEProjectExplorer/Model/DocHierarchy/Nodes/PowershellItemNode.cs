using System.Globalization;
using PsISEProjectExplorer.Enums;
using System;

namespace PsISEProjectExplorer.Model.DocHierarchy.Nodes
{
    public class PowershellItemNode : AbstractNode
    {
        public override NodeType NodeType { get { return NodeType.Function; } }

        public string FilePath { get; private set; }

        public PowershellItem PowershellItem { get; private set; }

        public PowershellItemNode(string filePath, PowershellItem item, INode parent)
            : base(GetNodePath(filePath, item), item.Name, parent)
        {
            this.FilePath = filePath;
            this.PowershellItem = item;
        }

        private static string GetNodePath(string filePath, PowershellItem item)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException("filePath");
            }
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            return System.IO.Path.Combine(filePath, item.StartLine.ToString() + "_" + item.StartColumn.ToString());
        }
    }
}
