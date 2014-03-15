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

        public static FileSystemNode CreateDirectoryOrFileNode(string path, string pathSegment, INode parent)
        {
            if ((File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory)
            {
                return new DirectoryNode(path, pathSegment, parent);
            }
            else
            {
                return new FileNode(path, pathSegment, parent);
            }

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
