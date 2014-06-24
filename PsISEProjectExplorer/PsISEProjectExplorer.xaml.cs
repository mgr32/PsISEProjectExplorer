using Microsoft.PowerShell.Host.ISE;
using NLog;
using NLog.Config;
using NLog.Targets;
using Ookii.Dialogs.Wpf;
using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.Services;
using PsISEProjectExplorer.UI;
using PsISEProjectExplorer.UI.Helpers;
using PsISEProjectExplorer.UI.IseIntegration;
using PsISEProjectExplorer.UI.ViewModel;
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace PsISEProjectExplorer
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ProjectExplorerWindow : IAddOnToolHostObject
    {
        private MainViewModel MainViewModel { get; set; }

        private Point DragStartPoint;

        private ObjectModelRoot hostObject { get; set; }

        private bool IsContextMenuOpened { get; set; }

         // Entry point to the ISE object model.
        public ObjectModelRoot HostObject
        {
            get { throw new InvalidOperationException("Should not use HostObject in user control - please use IseIntegrator class.");  }
            set { this.hostObject = value; OnHostObjectSet(); }
        }

        private void OnHostObjectSet()
        {
            this.MainViewModel.IseIntegrator = new IseIntegrator(this.hostObject);
        }

        public ProjectExplorerWindow()
        {
            this.ConfigureLogging();
            this.MainViewModel = new MainViewModel();
            this.MainViewModel.ActiveDocumentSyncEvent += OnActiveDocumentSyncEvent;
            this.DataContext = this.MainViewModel;
            InitializeComponent();
            this.Dispatcher.UnhandledExceptionFilter += DispatchUnhandledExceptionFilterHandler;
        }

        private static void DispatchUnhandledExceptionFilterHandler(object sender, DispatcherUnhandledExceptionFilterEventArgs args)
        {
            Exception e = (Exception)args.Exception;
            StringBuilder msg = new StringBuilder();
            msg.AppendLine("An unhandled exception occurred in PsISEProjectExplorer: ");
            msg.AppendLine(e.ToString());
            msg.AppendLine();
            var msgClipboard = msg.ToString();
            msg.AppendLine("Please create an issue at https://github.com/mgr32/PsISEProjectExplorer describing context of your action and pasting information stored in the clipboard.");
            Clipboard.SetText(msgClipboard.ToString());
            MessageBoxHelper.ShowError(msg.ToString());
            args.RequestCatch = false;
        }

        private void OnActiveDocumentSyncEvent(object sender, IseEventArgs args)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (this.MainViewModel.SyncWithActiveDocument)
                {
                    this.LocateFileInTree();
                }
            });
        }

        public void GoToDefinition()
        {
            Application.Current.Dispatcher.Invoke(() => this.MainViewModel.GoToDefinition());
        }

        public void FindAllOccurrences()
        {
            Application.Current.Dispatcher.Invoke(() => this.MainViewModel.FindAllOccurrences());
        }

        public void LocateFileInTree()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                string path = this.MainViewModel.IseIntegrator.SelectedFilePath;
                if (path == null)
                {
                    return;
                }
                var selectedItem = this.SearchResults.SelectedItem as TreeViewEntryItemModel;
                if (selectedItem != null && selectedItem.Path == path)
                {
                    return;
                }
                TreeViewEntryItemModel item = this.MainViewModel.TreeViewModel.FindTreeViewEntryItemByPath(path);
                if (item == null)
                {
                    return;
                }

                SearchResults.ExpandAndSelectItem(item);
            });
        }

        public void FindInFiles()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.MainViewModel.FindInFiles();
                this.TextBoxSearchText.Focus();
            });
        }

        public void CloseAllButThis()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.MainViewModel.IseIntegrator.CloseAllButThis();
            });
        }

        private void ChangeWorkspace_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog
            {
                SelectedPath = this.MainViewModel.RootDirectoryToSearch,
                Description = "Please select the new project root folder.",
                UseDescriptionForTitle = true
            };
            bool? dialogResult = dialog.ShowDialog();
            if (dialogResult != null && dialogResult.Value)
            {
                if (dialog.SelectedPath == Path.GetPathRoot(dialog.SelectedPath))
                {
                    MessageBoxHelper.ShowError("Cannot use root directory ('" + dialog.SelectedPath + "'). Please select another path.");
                }
                else
                {
                    this.MainViewModel.ChangeRootDirectory(dialog.SelectedPath);
                }
            }
        }

        private void RefreshDirectoryStructure_Click(object sender, RoutedEventArgs e)
        {
            this.MainViewModel.RecalculateRootDirectory(true);
        }

        private void ConfigureLogging()
        {
            #if DEBUG
            var config = new LoggingConfiguration();
            var target = new FileTarget
            {
                FileName = "C:\\PSIseProjectExplorer.log.txt",
                Layout = "${longdate}|${level:uppercase=true}|${logger}|${threadid}|${message}"
            };
            config.AddTarget("file", target);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, target));
            LogManager.Configuration = config;
            #endif
        }

        private void SearchResults_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = this.SearchResults.FindItemFromSource((DependencyObject)e.OriginalSource);
            if (item == null && this.SearchResults.SelectedItem != null)
            {
                ((TreeViewEntryItemModel)this.SearchResults.SelectedItem).IsSelected = false;
            }
            this.DragStartPoint = e.GetPosition(null);
            if (e.ClickCount > 1)
            {
                this.MainViewModel.TreeViewModel.OpenItem((TreeViewEntryItemModel)this.SearchResults.SelectedItem, this.MainViewModel.SearchText);
                e.Handled = true;
            }
        }

        private void SearchResults_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewEntryItemModel item;
            if (!this.SearchResults.SelectItemFromSource((DependencyObject)e.OriginalSource))
            {
                this.SearchResults.ContextMenu = this.SearchResults.Resources["EmptyContext"] as ContextMenu;
                item = (TreeViewEntryItemModel)this.SearchResults.SelectedItem;
                if (item != null)
                {
                    item.IsSelected = false;
                }
                return;
            }
            item = (TreeViewEntryItemModel)this.SearchResults.SelectedItem;
            if (item == null)
            {
                // should not happen
                this.SearchResults.ContextMenu = null;
            } else if (item.NodeType == Enums.NodeType.Directory)
            {
                this.SearchResults.ContextMenu = this.SearchResults.Resources["DirectoryContext"] as ContextMenu;
            }
            else if (item.NodeType == Enums.NodeType.File)
            {
                this.SearchResults.ContextMenu = this.SearchResults.Resources["FileContext"] as ContextMenu;
            }
            else
            {
                this.SearchResults.ContextMenu = null;
            }
        }

        private void SearchResults_KeyUp(object sender, KeyEventArgs e)
        {
            var selectedItem = (TreeViewEntryItemModel)this.SearchResults.SelectedItem;
            if (e.Key == Key.Enter)
            {
                this.MainViewModel.TreeViewModel.OpenItem(selectedItem, this.MainViewModel.SearchText);
            }
        }

        private void TextBoxSearchClear_Click(object sender, RoutedEventArgs e)
        {
            this.MainViewModel.SearchText = string.Empty;

        }

        private void SearchResults_AddDirectory(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewEntryItemModel)this.SearchResults.SelectedItem;
            this.MainViewModel.AddNewTreeItem(item, NodeType.Directory);
        }

        private void SearchResults_AddFile(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewEntryItemModel)this.SearchResults.SelectedItem;
            this.MainViewModel.AddNewTreeItem(item, NodeType.File);
        }

        private void SearchResults_Rename(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewEntryItemModel)this.SearchResults.SelectedItem;
            if (item == null)
            {
                return;
            }
            item.IsBeingEdited = true;
        }

        private void SearchResults_Delete(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewEntryItemModel)this.SearchResults.SelectedItem;
            if (item == null)
            {
                return;
            }
            this.MainViewModel.DeleteTreeItem(item);
        }

        private void SearchResults_EditKeyDown(object sender, KeyEventArgs e)
        {
            var item = (TreeViewEntryItemModel)this.SearchResults.SelectedItem;
            if (item == null)
            {
                return;
            }
            var newValue = ((TextBox)sender).Text;
            if (e.Key == Key.Escape)
            {
                this.MainViewModel.EndTreeEdit(newValue, false, item);
                e.Handled = true;
                return;
            }
            if (e.Key == Key.Enter)
            {
                this.MainViewModel.EndTreeEdit(newValue, true, item);
                e.Handled = true;
                return;
            }

        }

        private void SearchResults_KeyDown(object sender, KeyEventArgs e)
        {
            var item = (TreeViewEntryItemModel)this.SearchResults.SelectedItem;
            if (item == null || item.IsBeingEdited)
            {
                return;
            }
            if (e.Key == Key.Delete)
            {
                this.MainViewModel.DeleteTreeItem(item);
                e.Handled = true;
                return;
            }
            if (e.Key == Key.F2)
            {
                item.IsBeingEdited = true;
                e.Handled = true;
                return;
            }
            if (e.Key == Key.Enter)
            {
                this.MainViewModel.TreeViewModel.OpenItem((TreeViewEntryItemModel)this.SearchResults.SelectedItem, this.MainViewModel.SearchText);
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
            this.MainViewModel.EndTreeEdit(newValue, true, item);
        }

        private void SearchResults_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var selectedItem = this.SearchResults.SelectedItem as TreeViewEntryItemModel;
                if ((selectedItem != null && selectedItem.IsBeingEdited) || this.IsContextMenuOpened)
                {
                    return;
                }
                var mousePos = e.GetPosition(null);
                var diff = this.DragStartPoint - mousePos;

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
            }
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
                this.MainViewModel.MoveTreeItem(item, dropTarget);
            }
        }

        private void SearchResults_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(TreeViewEntryItemModel)))
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void SearchResults_ContextMenuClosed(object sender, RoutedEventArgs e)
        {
            this.IsContextMenuOpened = false;
        }

        private void SearchResults_ContextMenuOpened(object sender, RoutedEventArgs e)
        {
            this.IsContextMenuOpened = true;
        }

   }
}
