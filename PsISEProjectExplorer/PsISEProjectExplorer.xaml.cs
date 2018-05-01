using Microsoft.PowerShell.Host.ISE;
using NLog;
using PsISEProjectExplorer.Commands;
using PsISEProjectExplorer.Config;
using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.UI.Helpers;
using PsISEProjectExplorer.UI.ViewModel;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PsISEProjectExplorer
{
    /// <summary>
    /// Interaction logic for ProjectExplorerWindow.xaml
    /// </summary>
    public partial class ProjectExplorerWindow : IAddOnToolHostObject
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly CommandExecutor commandExecutor;
        private readonly DragDropHandler dragDropHandler;
        private readonly Bootstrap bootstrap;

        // Entry point to the ISE object model.
        public ObjectModelRoot HostObject
        {
            get { throw new InvalidOperationException("Should not use HostObject in user control - please use IseIntegrator class."); }
            set
            {
                this.bootstrap.Start(value);
            }
        }

        public StretchingTreeView SearchResultsTreeView
        {
            get
            {
                return this.SearchResults;
            }
        }

        public ProjectExplorerWindow()
        {
            ApplicationConfig applicationConfig = new ApplicationConfig();
            applicationConfig.ConfigureApplication(this);
            this.commandExecutor = applicationConfig.GetInstance<CommandExecutor>();
            this.dragDropHandler = applicationConfig.GetInstance<DragDropHandler>();
            this.bootstrap = applicationConfig.GetInstance<Bootstrap>();
            this.DataContext = applicationConfig.GetInstance<MainViewModel>();
            InitializeComponent();
        }

        public void FocusOnTextBoxSearchText()
        {
            this.TextBoxSearchText.Focus();
        }

        public void GoToDefinition()
        {
            this.commandExecutor.Execute<GoToDefinitionCommand>();
        }

        public void FindAllOccurrences()
        {
            this.commandExecutor.Execute<FindAllOccurrencesCommand>();
        }

        public void LocateFileInTree()
        {
            this.commandExecutor.Execute<LocateFileInTreeCommand>();
        }

        public void FindInFiles()
        {
            this.commandExecutor.Execute<FindInFilesCommand>();
        }

        public void CloseAllButThis()
        {
            this.commandExecutor.Execute<CloseAllButThisCommand>();
        }

        private void ChangeWorkspace_Click(object sender, RoutedEventArgs e)
        {
            this.commandExecutor.Execute<ChangeWorkspaceCommand>();
        }

        private void RefreshDirectoryStructure_Click(object sender, RoutedEventArgs e)
        {
            this.commandExecutor.ExecuteWithParam<ReindexSearchTreeCommand, IEnumerable<string>>(null);
        }

        private void CollapseAll_Click(object sender, RoutedEventArgs e)
        {
            this.commandExecutor.Execute<CollapseAllCommand>();
        }

        private void SearchResults_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var originalSource = (DependencyObject)e.OriginalSource;
            this.commandExecutor.ExecuteWithParam<SelectItemCommand, DependencyObject>(originalSource);
            this.dragDropHandler.DragStartPoint = e.GetPosition(null);
            if (e.ClickCount > 1)
            {
                this.commandExecutor.Execute<OpenItemCommand>();
                e.Handled = true;
            }
        }

        private void SearchResults_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.commandExecutor.ExecuteWithParam<OpenBuiltinContextMenuCommand, DependencyObject>((DependencyObject)e.OriginalSource);
        }

        private void SearchResults_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.commandExecutor.Execute<OpenItemCommand>();
            }
        }

        private void TextBoxSearchClear_Click(object sender, RoutedEventArgs e)
        {
            this.commandExecutor.Execute<ClearSearchTextCommand>();
        }

        private void SearchResults_AddDirectory(object sender, RoutedEventArgs e)
        {
            this.commandExecutor.ExecuteWithParam<AddNewTreeItemCommand, NodeType>(NodeType.Directory);
        }

        private void SearchResults_AddFile(object sender, RoutedEventArgs e)
        {
            this.commandExecutor.ExecuteWithParam<AddNewTreeItemCommand, NodeType>(NodeType.File);
        }

        private void SearchResults_ExcludeOrInclude(object sender, RoutedEventArgs e)
        {
            this.commandExecutor.Execute<ExcludeOrIncludeItemCommand>();
        }

        private void SearchResults_Rename(object sender, RoutedEventArgs e)
        {
            this.commandExecutor.Execute<RenameItemCommand>();
        }

        private void SearchResults_Delete(object sender, RoutedEventArgs e)
        {
            this.commandExecutor.Execute<DeleteItemCommand>();
        }

        private void SearchResults_EditKeyDown(object sender, KeyEventArgs e)
        {
            var item = (TreeViewEntryItemModel)this.SearchResults.SelectedItem;
            if (item == null)
            {
                return;
            }

            var newValue = ((TextBox)sender).Text;

            switch (e.Key)
            {
                case Key.Escape:
                    this.commandExecutor.ExecuteWithParam<EndEditingTreeItemCommand, Tuple<string, bool>>(Tuple.Create(newValue, false));
                    e.Handled = true;
                    return;
                case Key.Enter:
                    this.commandExecutor.ExecuteWithParam<EndEditingTreeItemCommand, Tuple<string, bool>>(Tuple.Create(newValue, true));
                    e.Handled = true;
                    return;
            }
        }

        private void SearchResults_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var item = (TreeViewEntryItemModel)this.SearchResults.SelectedItem;
            if (item == null || !item.IsBeingEdited)
            {
                return;
            }
            // this is another WPF workaround - see http://stackoverflow.com/questions/32265569/treeviewitem-with-textbox-in-wpf-type-special-characters
            this.SearchResults.RouteSpecialCharacters(sender, e);            
        }

        private void SearchResults_KeyDown(object sender, KeyEventArgs e)
        {
            var item = (TreeViewEntryItemModel)this.SearchResults.SelectedItem;
            if (item == null || item.IsBeingEdited)
            {
                return;
            }

            switch (e.Key)
            {
                case Key.Delete:
                    this.commandExecutor.Execute<DeleteItemCommand>();
                    e.Handled = true;
                    return;
                case Key.F2:
                    this.commandExecutor.Execute<RenameItemCommand>();
                    e.Handled = true;
                    return;
                case Key.F8:
                    this.commandExecutor.Execute<ExcludeOrIncludeItemCommand>();
                    e.Handled = true;
                    return;
                case Key.Enter:
                    this.commandExecutor.Execute<OpenItemCommand>();
                    e.Handled = true;
                    return;
                case Key.OemPeriod:
                    this.commandExecutor.Execute<DotSourceCommand>();
                    e.Handled = true;
                    return;
            }
        }

        private void SearchResults_EndEdit(object sender, RoutedEventArgs e)
        {
            var newValue = ((TextBox)sender).Text;
            this.commandExecutor.ExecuteWithParam<EndEditingTreeItemCommand, Tuple<string, bool>>(Tuple.Create(newValue, true));
        }

        private void SearchResults_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            this.dragDropHandler.HandleMouseMove(sender, e);
        }

        private void SearchResults_Drop(object sender, DragEventArgs e)
        {
            this.dragDropHandler.HandleDrop(sender, e);
        }

        private void SearchResults_DragEnter(object sender, DragEventArgs e)
        {
            this.dragDropHandler.HandleDragEnter(sender, e);
        }

        private void SearchResults_OpenInExplorer(object sender, RoutedEventArgs e)
        {
            this.commandExecutor.Execute<OpenInExplorerCommand>();
        }

        private void SearchResults_DotSource(object sender, RoutedEventArgs e)
        {
            this.commandExecutor.Execute<DotSourceCommand>();
        }

        private void SearchResults_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.dragDropHandler.ClearDragStartPoint();
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                // if ctrl is pressed - show builtin context menu
                return;
            }

            this.commandExecutor.Execute<OpenExplorerContextMenuCommand>();
        }
    }
}
