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
                        case PowershellItemType.Configuration: return NodeType.Configuration;
                        case PowershellItemType.Class: return NodeType.Class;
                        case PowershellItemType.ClassProperty: return NodeType.ClassProperty;
                        case PowershellItemType.ClassConstructor: return NodeType.ClassConstructor;
                        case PowershellItemType.DslElement: return NodeType.DslElement;
                        case PowershellItemType.Region: return NodeType.Region;   
                    }
                }
                return NodeType.Function;
            }
        }

        public string FilePath { get; private set; }

        public PowershellItemNode(string filePath, PowershellItem item, INode parent)
            : base(GetNodePath(filePath, item, parent), item.Name, parent, item)
        {
            this.FilePath = filePath;
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
