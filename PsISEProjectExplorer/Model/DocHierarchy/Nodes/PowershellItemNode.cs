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

        public PowershellItemNode(string filePath, PowershellItem func, INode parent)
            : base(GetNodePath(filePath, func), func.Name, parent)
        {
            this.FilePath = filePath;
            this.PowershellItem = func;
        }

        private static string GetNodePath(string filePath, PowershellItem func)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException("filePath");
            }
            if (func == null)
            {
                throw new ArgumentNullException("func");
            }

            return System.IO.Path.Combine(filePath, func.StartLine.ToString(CultureInfo.InvariantCulture));
        }
    }
}
