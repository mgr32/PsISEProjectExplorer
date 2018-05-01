using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using PsISEProjectExplorer.UI.Helpers;
using PsISEProjectExplorer.UI.IseIntegration;
using PsISEProjectExplorer.UI.ViewModel;
using System;
using System.Diagnostics;
using System.IO;

namespace PsISEProjectExplorer.Commands
{
    [Component]
    public class DotSourceCommand : Command
    {
        private readonly TreeViewModel treeViewModel;

        private readonly IseIntegrator iseIntegrator;

        public DotSourceCommand(TreeViewModel TreeViewModel, IseIntegrator iseIntegrator)
        {
            this.treeViewModel = TreeViewModel;
            this.iseIntegrator = iseIntegrator;
        }

        public void Execute()
        {
            var selectedItem = this.treeViewModel.SelectedItem;
            if (selectedItem == null)
            {
                return;
            }
            var fileItem = GetParentFileItem(selectedItem);
            if (fileItem == null || fileItem.Path == null || iseIntegrator.SelectedFilePath == null)
            {
                return;
            }
            string dotSourcePath = GetDotSourcePath(iseIntegrator.SelectedFilePath, fileItem.Path);
            iseIntegrator.WriteTextWithNewLine(String.Format(". \"$PSScriptRoot\\{0}\"", dotSourcePath));
            iseIntegrator.SetFocusOnCurrentTab();
        }

        private string GetDotSourcePath(string srcPath, string dstPath)
        {
            Uri srcUri = new Uri(srcPath);
            Uri dstUri = new Uri(dstPath);
            Uri relativeUri = srcUri.MakeRelativeUri(dstUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());
            if (dstUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }
            return relativePath;
        }

        private TreeViewEntryItemModel GetParentFileItem(TreeViewEntryItemModel selectedItem)
        {
            var item = selectedItem;
            while (item.NodeType != NodeType.File && item.Parent != null)
            {
                item = item.Parent;
            }
            if (item.NodeType == NodeType.File)
            {
                return item;
            }
            return null;
        }

    }
}
