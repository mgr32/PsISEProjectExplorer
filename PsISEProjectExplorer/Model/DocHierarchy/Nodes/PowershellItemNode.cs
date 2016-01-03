using PsISEProjectExplorer.Enums;
using System;

namespace PsISEProjectExplorer.Model.DocHierarchy.Nodes
{
    public class PowershellItemNode : AbstractNode
    {
        public override NodeType NodeType { get
            {
                if (PowershellItem != null)
                {
                    switch (PowershellItem.Type)
                    {
                        case PowershellItemType.Class: return NodeType.Class;
                        case PowershellItemType.ClassProperty: return NodeType.ClassProperty;
                        case PowershellItemType.ClassConstructor: return NodeType.ClassConstructor;
                        case PowershellItemType.DslElement: return NodeType.DslElement;
                    }
                }
                return NodeType.Function;
            }
        }

        public string FilePath { get; private set; }

        public PowershellItem PowershellItem { get; private set; }

        public PowershellItemNode(string filePath, PowershellItem item, INode parent)
            : base(GetNodePath(filePath, item, parent), item.Name, parent)
        {
            this.FilePath = filePath;
            this.PowershellItem = item;
        }

        private static string GetNodePath(string filePath, PowershellItem item, INode parent)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException("filePath");
            }
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            string basePath = parent != null && parent is PowershellItemNode ? parent.Path : filePath;
            return System.IO.Path.Combine(basePath, item.StartLine.ToString() + "_" + item.StartColumn.ToString());
        }
    }
}
