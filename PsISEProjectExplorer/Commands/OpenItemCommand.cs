using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.Model;
using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using PsISEProjectExplorer.Services;
using PsISEProjectExplorer.UI.IseIntegration;
using PsISEProjectExplorer.UI.ViewModel;
using System;

namespace PsISEProjectExplorer.Commands
{
    [Component]
    public class OpenItemCommand : Command
    {
        private readonly TreeViewModel treeViewModel;

        private readonly MainViewModel mainViewModel;

        private readonly IseIntegrator iseIntegrator;

        private readonly TokenLocator tokenLocator;

        public OpenItemCommand(TreeViewModel treeViewModel, MainViewModel mainViewModel, IseIntegrator iseIntegrator, TokenLocator tokenLocator)
        {
            this.treeViewModel = treeViewModel;
            this.mainViewModel = mainViewModel;
            this.iseIntegrator = iseIntegrator;
            this.tokenLocator = tokenLocator;
        }

        public void Execute()
        {
            var item = this.treeViewModel.SelectedItem;
            var searchOptions = this.mainViewModel.SearchOptions;
            if (item == null)
            {
                return;
            }

            if (item.Node.NodeType == NodeType.File)
            {
                bool wasOpen = (this.iseIntegrator.SelectedFilePath == item.Node.Path);
                if (!wasOpen)
                {
                    this.iseIntegrator.GoToFile(item.Node.Path);
                }
                else
                {
                    this.iseIntegrator.SetFocusOnCurrentTab();
                }
                if (searchOptions.SearchText != null && searchOptions.SearchText.Length > 2)
                {
                    EditorInfo editorInfo = (wasOpen ? this.iseIntegrator.GetCurrentLineWithColumnIndex() : null);
                    TokenPosition tokenPos = this.tokenLocator.LocateNextToken(item.Node.Path, searchOptions, editorInfo);
                    if (tokenPos.MatchLength > 2)
                    {
                        this.iseIntegrator.SelectText(tokenPos.Line, tokenPos.Column, tokenPos.MatchLength);
                    }
                    else if (string.IsNullOrEmpty(this.iseIntegrator.SelectedText))
                    {
                        tokenPos = this.tokenLocator.LocateSubtoken(item.Node.Path, searchOptions);
                        if (tokenPos.MatchLength > 2)
                        {
                            this.iseIntegrator.SelectText(tokenPos.Line, tokenPos.Column, tokenPos.MatchLength);
                        }
                    }
                }
            }
            else if (item.Node.NodeType == NodeType.Directory)
            {
                item.IsExpanded = !item.IsExpanded;
            }
            else if (item.Node.NodeType != NodeType.Intermediate)
            {
                var node = ((PowershellItemNode)item.Node);
                this.iseIntegrator.GoToFile(node.FilePath);
                this.iseIntegrator.SelectText(node.PowershellItem.StartLine, node.PowershellItem.StartColumn, node.PowershellItem.EndColumn - node.PowershellItem.StartColumn);
            }

        }
    }
}
