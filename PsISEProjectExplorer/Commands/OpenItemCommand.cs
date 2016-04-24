using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.Model;
using PsISEProjectExplorer.Model.DocHierarchy.Nodes;
using PsISEProjectExplorer.Services;
using PsISEProjectExplorer.UI.IseIntegration;
using PsISEProjectExplorer.UI.ViewModel;
using System;

namespace PsISEProjectExplorer.Commands
{
    public class OpenItemCommand : Command
    {
        private TreeViewModel TreeViewModel { get; set; }

        private MainViewModel MainViewModel { get; set; }

        private IseIntegrator IseIntegrator { get; set; }

        private TokenLocator TokenLocator { get; set; }

        public OpenItemCommand(TreeViewModel treeViewModel, MainViewModel mainViewModel, IseIntegrator iseIntegrator, TokenLocator tokenLocator)
        {
            this.TreeViewModel = treeViewModel;
            this.MainViewModel = mainViewModel;
            this.IseIntegrator = iseIntegrator;
            this.TokenLocator = tokenLocator;
        }

        public void Execute()
        {
            var item = this.TreeViewModel.SelectedItem;
            var searchOptions = this.MainViewModel.SearchOptions;
            if (item == null)
            {
                return;
            }

            if (item.Node.NodeType == NodeType.File)
            {
                bool wasOpen = (this.IseIntegrator.SelectedFilePath == item.Node.Path);
                if (!wasOpen)
                {
                    this.IseIntegrator.GoToFile(item.Node.Path);
                }
                else
                {
                    this.IseIntegrator.SetFocusOnCurrentTab();
                }
                if (searchOptions.SearchText != null && searchOptions.SearchText.Length > 2)
                {
                    EditorInfo editorInfo = (wasOpen ? this.IseIntegrator.GetCurrentLineWithColumnIndex() : null);
                    TokenPosition tokenPos = this.TokenLocator.LocateNextToken(item.Node.Path, searchOptions, editorInfo);
                    if (tokenPos.MatchLength > 2)
                    {
                        this.IseIntegrator.SelectText(tokenPos.Line, tokenPos.Column, tokenPos.MatchLength);
                    }
                    else if (string.IsNullOrEmpty(this.IseIntegrator.SelectedText))
                    {
                        tokenPos = this.TokenLocator.LocateSubtoken(item.Node.Path, searchOptions);
                        if (tokenPos.MatchLength > 2)
                        {
                            this.IseIntegrator.SelectText(tokenPos.Line, tokenPos.Column, tokenPos.MatchLength);
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
                this.IseIntegrator.GoToFile(node.FilePath);
                this.IseIntegrator.SelectText(node.PowershellItem.StartLine, node.PowershellItem.StartColumn, node.PowershellItem.EndColumn - node.PowershellItem.StartColumn);
            }

        }
    }
}
