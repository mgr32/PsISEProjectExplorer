using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectExplorer.DocHierarchy.Nodes
{
    public abstract class FileSystemNode : AbstractNode
    {

        public FileSystemNode(string path, string name, INode parent) : base(path, name, parent)
        {
        }

        /*public DocumentHierarchyFileSystemNode FindChildByPathSegment(string pathSegment)
        {
            foreach (var child in this.Children)
            {
                var childFileSystemNode = child as DocumentHierarchyFileSystemNode;
                if (childFileSystemNode != null)
                {
                    if (childFileSystemNode.PathSegment == pathSegment)
                    {
                        return childFileSystemNode;
                    }
                }
            }
            return null;
        }
        */
    }
}
