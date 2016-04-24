using Microsoft.PowerShell.Host.ISE;
using NLog;
using PsISEProjectExplorer.Commands;
using PsISEProjectExplorer.Config;
using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.UI.Helpers;
using PsISEProjectExplorer.UI.IseIntegration;
using PsISEProjectExplorer.UI.ViewModel;
using System;
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

        private MainViewModel MainViewModel { get; set; }
        private CommandExecutor CommandExecutor { get; set; }
        private DragDropHandler DragDropHandler { get; set; }

        // Entry point to the ISE object model.
        public ObjectModelRoot HostObject
        {
            get { throw new InvalidOperationException("Should not use HostObject in user control - please use IseIntegrator class."); }
            set
            {
                this.MainViewModel.setIseHostObject(value);
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
            BootstrapConfig bootstrapConfig = new BootstrapConfig();
            bootstrapConfig.ConfigureApplication(this);
            this.MainViewModel = bootstrapConfig.GetInstance<MainViewModel>();
            this.MainViewModel.ActiveDocumentSyncEvent += OnActiveDocumentSyncEvent;
            this.CommandExecutor = bootstrapConfig.GetInstance<CommandExecutor>();
            this.DragDropHandler = bootstrapConfig.GetInstance<DragDropHandler>();
            this.DataContext = this.MainViewModel;
            InitializeComponent();
        }

        public void FocusOnTextBoxSearchText()
        {
            this.TextBoxSearchText.Focus();
        }

        private void OnActiveDocumentSyncEvent(object sender, IseEventArgs args)
        {
            if (this.MainViewModel.SyncWithActiveDocument)
            {
                this.LocateFileInTree();
            }
        }

        public void GoToDefinition()
        {
            this.CommandExecutor.Execute<GoToDefinitionCommand>();
        }

        public void FindAllOccurrences()
        {
            this.CommandExecutor.Execute<FindAllOccurrencesCommand>();
        }

        public void LocateFileInTree()
        {
            this.CommandExecutor.Execute<LocateFileInTreeCommand>();
        }

        public void FindInFiles()
        {
            this.CommandExecutor.Execute<FindInFilesCommand>();
        }

        public void CloseAllButThis()
        {
            this.CommandExecutor.Execute<CloseAllButThisCommand>();
        }

        private void ChangeWorkspace_Click(object sender, RoutedEventArgs e)
        {
            this.CommandExecutor.Execute<ChangeWorkspaceCommand>();
        }

        private void RefreshDirectoryStructure_Click(object sender, RoutedEventArgs e)
        {
            this.CommandExecutor.Execute<RefreshDirectoryStructureCommand>();
        }

        private void CollapseAll_Click(object sender, RoutedEventArgs e)
        {
            this.CommandExecutor.Execute<CollapseAllCommand>();
        }

        private void SearchResults_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var originalSource = (DependencyObject)e.OriginalSource;
            this.CommandExecutor.ExecuteWithParam<SelectItemCommand, DependencyObject>(originalSource);
            this.DragDropHandler.DragStartPoint = e.GetPosition(null);
            if (e.ClickCount > 1)
            {
                this.CommandExecutor.Execute<OpenItemCommand>();
                e.Handled = true;
            }
        }

        private void SearchResults_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.CommandExecutor.ExecuteWithParam<OpenBuiltinContextMenuCommand, DependencyObject>((DependencyObject)e.OriginalSource);
        }

        private void SearchResults_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.CommandExecutor.Execute<OpenItemCommand>();
            }
        }

        private void TextBoxSearchClear_Click(object sender, RoutedEventArgs e)
        {
            this.MainViewModel.SearchText = string.Empty;
        }

        private void SearchResults_AddDirectory(object sender, RoutedEventArgs e)
        {
            this.CommandExecutor.ExecuteWithParam<AddNewTreeItemCommand, NodeType>(NodeType.Directory);
        }

        private void SearchResults_AddFile(object sender, RoutedEventArgs e)
        {
            this.CommandExecutor.ExecuteWithParam<AddNewTreeItemCommand, NodeType>(NodeType.File);
        }

        private void SearchResults_ExcludeOrInclude(object sender, RoutedEventArgs e)
        {
            this.CommandExecutor.Execute<ExcludeOrIncludeItemCommand>();
        }

        private void SearchResults_Rename(object sender, RoutedEventArgs e)
        {
            this.CommandExecutor.Execute<RenameItemCommand>();
        }

        private void SearchResults_Delete(object sender, RoutedEventArgs e)
        {
            this.CommandExecutor.Execute<DeleteItemCommand>();
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
                    this.MainViewModel.TreeViewModel.EndTreeEdit(newValue, false, item, !this.MainViewModel.SearchInFiles);
                    e.Handled = true;
                    return;
                case Key.Enter:
                    this.MainViewModel.TreeViewModel.EndTreeEdit(newValue, true, item, !this.MainViewModel.SearchInFiles);
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
                    this.CommandExecutor.Execute<DeleteItemCommand>();
                    e.Handled = true;
                    return;
                case Key.F2:
                    this.CommandExecutor.Execute<RenameItemCommand>();
                    e.Handled = true;
                    return;
                case Key.F8:
                    this.CommandExecutor.Execute<ExcludeOrIncludeItemCommand>();
                    e.Handled = true;
                    return;
                case Key.Enter:
                    this.CommandExecutor.Execute<OpenItemCommand>();
                    e.Handled = true;
                    return;
            }
        }

        private void SearchResults_EndEdit(object sender, RoutedEventArgs e)
        {
            var newValue = ((TextBox)sender).Text;
            this.CommandExecutor.ExecuteWithParam<EndEditingTreeItemCommand, string>(newValue);
        }

        private void SearchResults_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            this.DragDropHandler.HandleMouseMove(sender, e);
        }

        private void SearchResults_Drop(object sender, DragEventArgs e)
        {
            this.DragDropHandler.HandleDrop(sender, e);
        }

        private void SearchResults_DragEnter(object sender, DragEventArgs e)
        {
            this.DragDropHandler.HandleDragEnter(sender, e);
        }

        private void SearchResults_OpenInExplorer(object sender, RoutedEventArgs e)
        {
            this.CommandExecutor.Execute<OpenInExplorerCommand>();
        }

        private void SearchResults_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.DragDropHandler.ClearDragStartPoint();
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                // if ctrl is pressed - show builtin context menu
                return;
            }

            this.CommandExecutor.Execute<OpenExplorerContextMenuCommand>();
        }
    }
}
