﻿using PsISEProjectExplorer.Model;
using PsISEProjectExplorer.Model.DocHierarchy;
using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PsISEProjectExplorer.Services
{
    public class DocumentHierarchyIndexer
    {
        private DocumentHierarchy DocumentHierarchy { get; set; }

        private INode RootNode 
        {   get
            {
                return this.DocumentHierarchy.RootNode;
            }
        }

        public DocumentHierarchyIndexer(DocumentHierarchy documentHierarchy)
        {
            this.DocumentHierarchy = documentHierarchy;
            
        }

        public bool AddFileSystemNode(PowershellParseResult parseResult)
        {
            if (this.DocumentHierarchy.GetNode(parseResult.Path) != null || parseResult.Path == this.DocumentHierarchy.RootNode.Path)
            {
                return false;
            }
            INode lastDirNode = this.FillHierarchyWithIntermediateDirectories(parseResult.Path, parseResult.IsDirectory, parseResult.IsExcluded, parseResult.ErrorMessage);
            if (!parseResult.IsDirectory)
            {
                FileNode fileNode = this.DocumentHierarchy.CreateNewFileNode(parseResult.Path, parseResult.FileContents, lastDirNode, parseResult.IsExcluded, parseResult.ErrorMessage);
                if (parseResult.RootPowershellItem != null)
                {
                    this.DocumentHierarchy.CreateNewPowershellItemNode(parseResult.Path, parseResult.RootPowershellItem, fileNode);
                    var parent = fileNode.Parent;
                    while (parent != null && parent is DirectoryNode)
                    {
                        if (parseResult.RootPowershellItem.ParsingErrors != null)
                        {
                            ((DirectoryNode)parent).AddFileError(fileNode.Name);
                        }
                        else
                        {
                            ((DirectoryNode)parent).RemoveFileError(fileNode.Name);
                        }
                        parent = parent.Parent;
                    }
                }
            }
            return true;
        }

        private INode FillHierarchyWithIntermediateDirectories(string path, bool lastSegmentIsDirectory, bool isExcluded, string errorMessage)
        {
            IList<string> segments = path.Replace(this.RootNode.Path + "\\", "").Split('\\').ToList();
            var currentNode = this.RootNode;
            if (!lastSegmentIsDirectory)
            {
                if (segments.Count <= 1)
                {
                    return currentNode;
                }
                segments.RemoveAt(segments.Count - 1);
                isExcluded = false;
            }
            var currentAbsolutePath = this.RootNode.Path;
            int lastIndex = segments.Count - 1;
            int i = 0;
            foreach (string segment in segments)
            {
                currentAbsolutePath = Path.Combine(currentAbsolutePath, segment);
                bool nodeIsExcluded = i == lastIndex ? isExcluded : false;
                currentNode = this.DocumentHierarchy.GetNode(currentAbsolutePath) ??
                    this.DocumentHierarchy.CreateNewDirectoryNode(currentAbsolutePath, currentNode, nodeIsExcluded, currentAbsolutePath == path ? errorMessage : null);
                i++;
            }
            return currentNode;
        }
    }
}
