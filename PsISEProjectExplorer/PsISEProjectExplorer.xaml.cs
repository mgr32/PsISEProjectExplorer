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

        private readonly MainViewModel mainViewModel;
        private readonly CommandExecutor commandExecutor;
        private Point dragStartPoint;


        // Entry point to the ISE object model.
        public ObjectModelRoot HostObject
        {
            get { throw new InvalidOperationException("Should not use HostObject in user control - please use IseIntegrator class."); }
            set
            {
                this.mainViewModel.setIseHostObject(value);
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
            this.mainViewModel = bootstrapConfig.GetInstance<MainViewModel>();
            this.mainViewModel.ActiveDocumentSyncEvent += OnActiveDocumentSyncEvent;
            this.commandExecutor = bootstrapConfig.GetInstance<CommandExecutor>();
            this.DataContext = this.mainViewModel;
            InitializeComponent();
        }

        public void FocusOnTextBoxSearchText()
        {
            this.TextBoxSearchText.Focus();
        }

        private void OnActiveDocumentSyncEvent(object sender, IseEventArgs args)
        {
            if (this.mainViewModel.SyncWithActiveDocument)
            {
                this.LocateFileInTree();
            }
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
            this.commandExecutor.Execute<RefreshDirectoryStructureCommand>();
        }

        private void CollapseAll_Click(object sender, RoutedEventArgs e)
        {
            this.commandExecutor.Execute<CollapseAllCommand>();
        }

        private void SearchResults_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var originalSource = (DependencyObject)e.OriginalSource;
            this.commandExecutor.ExecuteWithParam<SelectItemCommand, DependencyObject>(originalSource);
            this.dragStartPoint = e.GetPosition(null);
            if (e.ClickCount > 1)
            {
                this.commandExecutor.Execute<OpenItemCommand>();
                e.Handled = true;
            }
        }

        private void SearchResults_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var originalSource = (DependencyObject)e.OriginalSource;
            this.commandExecutor.ExecuteWithParam<OpenContextMenuCommand, DependencyObject>(originalSource);
        }


        private void SearchResults_KeyUp(object sender, KeyEventArgs e)
        {
            var selectedItem = (TreeViewEntryItemModel)this.SearchResults.SelectedItem;
            if (e.Key == Key.Enter)
            {
                this.mainViewModel.TreeViewModel.OpenItem(selectedItem, this.mainViewModel.SearchOptions);
            }
        }

        private void TextBoxSearchClear_Click(object sender, RoutedEventArgs e)
        {
            this.mainViewModel.SearchText = string.Empty;
        }

        private void SearchResults_AddDirectory(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewEntryItemModel)this.SearchResults.SelectedItem;
            this.mainViewModel.TreeViewModel.AddNewTreeItem(item, NodeType.Directory);
        }

        private void SearchResults_AddFile(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewEntryItemModel)this.SearchResults.SelectedItem;
            this.mainViewModel.TreeViewModel.AddNewTreeItem(item, NodeType.File);
        }

        private void SearchResults_ExcludeOrInclude(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewEntryItemModel)this.SearchResults.SelectedItem;
            if (item == null || item.IsBeingEdited)
            {
                return;
            }
            this.mainViewModel.ExcludeOrIncludeItem(item);
        }

        private void SearchResults_Rename(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewEntryItemModel)this.SearchResults.SelectedItem;
            if (item == null)
            {
                return;
            }

            this.mainViewModel.TreeViewModel.StartEditingTreeItem(item);
        }

        private void SearchResults_Delete(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewEntryItemModel)this.SearchResults.SelectedItem;
            if (item == null)
            {
                return;
            }

            this.mainViewModel.TreeViewModel.DeleteTreeItem(item);
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
                    this.mainViewModel.TreeViewModel.EndTreeEdit(newValue, false, item, !this.mainViewModel.SearchInFiles);
                    e.Handled = true;
                    return;
                case Key.Enter:
                    this.mainViewModel.TreeViewModel.EndTreeEdit(newValue, true, item, !this.mainViewModel.SearchInFiles);
                    e.Handled = true;
                    return;
            }
        }

        private void SearchResults_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // this is another WPF workaround - see http://stackoverflow.com/questions/32265569/treeviewitem-with-textbox-in-wpf-type-special-characters
            var item = (TreeViewEntryItemModel)this.SearchResults.SelectedItem;
            if (item == null || !item.IsBeingEdited)
            {
                return;
            }
            
            string keyText = null;
            switch (e.Key)
            {
                case Key.Subtract: keyText = "-"; break;
                case Key.Add: keyText = "+"; break;
                case Key.Multiply: keyText = "*"; break;
            }
            if (keyText == null)
            {
                return;
            }

            var target = Keyboard.FocusedElement;
            if (target == null)
            {
                return;
            }
            e.Handled = true;
            var routedEvent = TextCompositionManager.TextInputEvent;
            target.RaiseEvent(
                new TextCompositionEventArgs
                    (
                        InputManager.Current.PrimaryKeyboardDevice,
                        new TextComposition(InputManager.Current, target, keyText)
                    )
                {
                    RoutedEvent = routedEvent
                });
            
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
                    this.mainViewModel.TreeViewModel.DeleteTreeItem(item);
                    e.Handled = true;
                    return;
                case Key.F2:
                    this.mainViewModel.TreeViewModel.StartEditingTreeItem(item);
                    e.Handled = true;
                    return;
                case Key.F8:
                    this.mainViewModel.ExcludeOrIncludeItem(item);
                    e.Handled = true;
                    return;
                case Key.Enter:
                    this.mainViewModel.TreeViewModel.OpenItem((TreeViewEntryItemModel)this.SearchResults.SelectedItem, this.mainViewModel.SearchOptions);
                    e.Handled = true;
                    return;
            }
        }

        private void SearchResults_EndEdit(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewEntryItemModel)this.SearchResults.SelectedItem;
            if (item == null)
            {
                return;
            }

            var newValue = ((TextBox)sender).Text;
            this.mainViewModel.TreeViewModel.EndTreeEdit(newValue, true, item, !this.mainViewModel.SearchInFiles);
        }

        private void SearchResults_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var selectedItem = this.SearchResults.SelectedItem as TreeViewEntryItemModel;
                if ((selectedItem != null && selectedItem.IsBeingEdited))
                {
                    return;
                }
                if (this.isDragStartPointEmpty())
                {
                    return;
                }

                var mousePos = e.GetPosition(null);
                var diff = this.dragStartPoint - mousePos;

                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance
                    || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    var treeView = sender as TreeView;
                    if (treeView == null)
                    {
                        return;
                    }

                    var treeViewItem = treeView.FindItemFromSource((DependencyObject)e.OriginalSource);
                    if (treeViewItem == null)
                    {
                        return;
                    }

                    var item = treeView.SelectedItem as TreeViewEntryItemModel;
                    if (item == null)
                    {
                        return;
                    }

                    var dragData = new DataObject(item);
                    DragDrop.DoDragDrop(treeViewItem, dragData, DragDropEffects.Move);
                }
            } else if (!isDragStartPointEmpty())
            {
                clearDragStartPoint();
            }
        }

        private Boolean isDragStartPointEmpty()
        {
            return this.dragStartPoint.X == 0.0 && this.dragStartPoint.Y == 0.0;
        }

        private void clearDragStartPoint()
        {
            this.dragStartPoint.X = 0.0;
            this.dragStartPoint.Y = 0.0;
        }

        private void SearchResults_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(TreeViewEntryItemModel)))
            {
                var item = e.Data.GetData(typeof(TreeViewEntryItemModel)) as TreeViewEntryItemModel;
                var treeView = sender as TreeView;

                if (treeView == null || item == null)
                {
                    return;
                }

                var treeViewItem = treeView.FindItemFromSource((DependencyObject)e.OriginalSource);

                TreeViewEntryItemModel dropTarget = null;

                if (treeViewItem != null)
                {
                    dropTarget = treeViewItem.Header as TreeViewEntryItemModel;
                    if (dropTarget == null)
                    {
                        return;
                    }
                }

                if (item != dropTarget)
                {
                    this.mainViewModel.TreeViewModel.MoveTreeItem(item, dropTarget, this.mainViewModel.WorkspaceDirectoryModel.CurrentWorkspaceDirectory);
                }
            }
            this.clearDragStartPoint();
        }

        private void SearchResults_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(TreeViewEntryItemModel)))
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void SearchResults_OpenInExplorer(object sender, RoutedEventArgs e)
        {
            this.commandExecutor.Execute<OpenInExplorerCommand>();
        }

        private void SearchResults_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            clearDragStartPoint();
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                // if ctrl is pressed - show builtin context menu
                return;
            }

            this.commandExecutor.Execute<OpenExplorerContextMenuCommand>();
        }
    }
}
