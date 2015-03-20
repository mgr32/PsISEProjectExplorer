﻿using Microsoft.PowerShell.Host.ISE;
using NLog;
using NLog.Targets;
using Ookii.Dialogs.Wpf;
using PsISEProjectExplorer.Enums;
using PsISEProjectExplorer.UI.Helpers;
using PsISEProjectExplorer.UI.IseIntegration;
using PsISEProjectExplorer.UI.ViewModel;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static string LogFileName { get; set; }

        private MainViewModel MainViewModel { get; set; }

        private Point DragStartPoint;

        private ObjectModelRoot hostObject { get; set; }

        private bool IsContextMenuOpened { get; set; }

         // Entry point to the ISE object model.
        public ObjectModelRoot HostObject
        {
            get { throw new InvalidOperationException("Should not use HostObject in user control - please use IseIntegrator class.");  }
            set { hostObject = value; OnHostObjectSet(); }
        }

        private void OnHostObjectSet()
        {
			MainViewModel.IseIntegrator = new IseIntegrator(hostObject);
        }

        public ProjectExplorerWindow()
        {
			ConfigureLogging();
			MainViewModel = new MainViewModel();
			MainViewModel.ActiveDocumentSyncEvent += OnActiveDocumentSyncEvent;
			DataContext = MainViewModel;
            InitializeComponent();
			Dispatcher.UnhandledException += DispatcherUnhandledExceptionHandler;
        }

        private static void DispatcherUnhandledExceptionHandler(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.Exception;
            Logger.Error("Unhandled Dispatcher exception", e);

            StringBuilder sources = new StringBuilder().Append("Sources: ");
            string firstSource = null;
            var innerException = e.InnerException;

            while (innerException != null)
            {
                if (firstSource == null)
                {
                    firstSource = innerException.Source;
                }
                sources.Append(innerException.Source).Append(",");
                innerException = innerException.InnerException;
            }
            Logger.Error(sources.ToString());
            StringBuilder msg = new StringBuilder();
            msg.AppendLine("An unhandled exception occurred in Powershell ISE - please save your work and restart ISE as soon as possible. ");
            msg.AppendLine();
            if (firstSource != null && firstSource.ToLowerInvariant().Equals("psiseprojectexplorer"))
            {
                msg.AppendLine("This is most likely PsISEProjectExplorer error. Please create an issue at https://github.com/mgr32/PsISEProjectExplorer.");
            }
            else
            {
                msg.AppendLine("Note this information comes from PsISEProjectExplorer, but the exception has been probably thrown from another module or from ISE itself.");
            }
            msg.AppendLine();
            msg.AppendLine("Exception: " + e.Message);
            if (e.InnerException != null)
            {
                msg.AppendLine("Inner exception source: " + e.InnerException.Source);
                msg.AppendLine("Inner exception message: " + e.InnerException.Message);
                msg.AppendLine("Inner exception site: " + e.InnerException.TargetSite);
            }
            msg.AppendLine();
            msg.AppendLine("More details can be found in log file at: " + LogFileName);
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(Application.Current.MainWindow, msg.ToString(), "Powershell ISE error", MessageBoxButton.OK, MessageBoxImage.Error);
            });
            args.Handled = true;
        }

        private void OnActiveDocumentSyncEvent(object sender, IseEventArgs args)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (MainViewModel.SyncWithActiveDocument)
                {
					LocateFileInTree();
                }
            });
        }

        public void GoToDefinition()
        {
            Application.Current.Dispatcher.Invoke(() => MainViewModel.GoToDefinition());
        }

        public void FindAllOccurrences()
        {
            Application.Current.Dispatcher.Invoke(() => MainViewModel.FindAllOccurrences());
        }

        public void LocateFileInTree()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                string path = MainViewModel.IseIntegrator.SelectedFilePath;
                if (path == null)
                {
                    return;
                }
                var selectedItem = SearchResults.SelectedItem as TreeViewEntryItemModel;
                if (selectedItem != null && selectedItem.Path.StartsWith(path))
                {
                    return;
                }
                TreeViewEntryItemModel item = MainViewModel.TreeViewModel.FindTreeViewEntryItemByPath(path);
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
				MainViewModel.FindInFiles();
				TextBoxSearchText.Focus();
            });
        }

        public void CloseAllButThis()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
				MainViewModel.IseIntegrator.CloseAllButThis();
            });
        }

        private void ChangeWorkspace_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog
            {
                SelectedPath = MainViewModel.WorkspaceDirectoryModel.CurrentWorkspaceDirectory,
                Description = "Please select the new workspace folder.",
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
					MainViewModel.WorkspaceDirectoryModel.SetWorkspaceDirectory(dialog.SelectedPath);
					MainViewModel.WorkspaceDirectoryModel.AutoUpdateRootDirectory = false;
                }
            }
        }

        private void RefreshDirectoryStructure_Click(object sender, RoutedEventArgs e)
        {
			MainViewModel.ReindexSearchTree();
        }

        private void ConfigureLogging()
        {
            string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            LogFileName = Path.Combine(assemblyFolder, "NLog.config");
            var config = new NLog.Config.XmlLoggingConfiguration(LogFileName);
            LogManager.Configuration = config;
            var targets = config.AllTargets;
            if (targets != null && targets.Any() && targets.First() is FileTarget) {
                LogFileName = ((FileTarget)targets.First()).FileName.Render(new LogEventInfo());
            }

        }

        private void SearchResults_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = SearchResults.FindItemFromSource((DependencyObject)e.OriginalSource);
            if (item == null && SearchResults.SelectedItem != null)
            {
                ((TreeViewEntryItemModel)SearchResults.SelectedItem).IsSelected = false;
            }
			DragStartPoint = e.GetPosition(null);
            if (e.ClickCount > 1)
            {
				MainViewModel.TreeViewModel.OpenItem((TreeViewEntryItemModel)SearchResults.SelectedItem, MainViewModel.SearchText);
                e.Handled = true;
            }
        }

        private void SearchResults_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewEntryItemModel item;
            if (!SearchResults.SelectItemFromSource((DependencyObject)e.OriginalSource))
            {
				SearchResults.ContextMenu = SearchResults.Resources["EmptyContext"] as ContextMenu;
                item = (TreeViewEntryItemModel)SearchResults.SelectedItem;
                if (item != null)
                {
                    item.IsSelected = false;
                }
                return;
            }
            item = (TreeViewEntryItemModel)SearchResults.SelectedItem;
            if (item == null)
            {
				// should not happen
				SearchResults.ContextMenu = null;
            } else if (item.NodeType == Enums.NodeType.Directory)
            {
				SearchResults.ContextMenu = SearchResults.Resources["DirectoryContext"] as ContextMenu;
            }
            else if (item.NodeType == Enums.NodeType.File)
            {
				SearchResults.ContextMenu = SearchResults.Resources["FileContext"] as ContextMenu;
            }
            else
            {
				SearchResults.ContextMenu = null;
            }
        }

        private void SearchResults_KeyUp(object sender, KeyEventArgs e)
        {
            var selectedItem = (TreeViewEntryItemModel)SearchResults.SelectedItem;
            if (e.Key == Key.Enter)
            {
				MainViewModel.TreeViewModel.OpenItem(selectedItem, MainViewModel.SearchText);
            }
        }

        private void TextBoxSearchClear_Click(object sender, RoutedEventArgs e)
        {
			MainViewModel.SearchText = string.Empty;

        }

        private void SearchResults_AddDirectory(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewEntryItemModel)SearchResults.SelectedItem;
			MainViewModel.TreeViewModel.AddNewTreeItem(item, NodeType.Directory);
        }

        private void SearchResults_AddFile(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewEntryItemModel)SearchResults.SelectedItem;
			MainViewModel.TreeViewModel.AddNewTreeItem(item, NodeType.File);
        }

        private void SearchResults_Rename(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewEntryItemModel)SearchResults.SelectedItem;
            if (item == null)
            {
                return;
            }
			MainViewModel.TreeViewModel.StartEditingTreeItem(item);
        }

        private void SearchResults_Delete(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewEntryItemModel)SearchResults.SelectedItem;
            if (item == null)
            {
                return;
            }
			MainViewModel.TreeViewModel.DeleteTreeItem(item);
        }

        private void SearchResults_EditKeyDown(object sender, KeyEventArgs e)
        {
            var item = (TreeViewEntryItemModel)SearchResults.SelectedItem;
            if (item == null)
            {
                return;
            }
            var newValue = ((TextBox)sender).Text;
            if (e.Key == Key.Escape)
            {
				MainViewModel.TreeViewModel.EndTreeEdit(newValue, false, item, !MainViewModel.SearchInFiles);
                e.Handled = true;
                return;
            }
            if (e.Key == Key.Enter)
            {
				MainViewModel.TreeViewModel.EndTreeEdit(newValue, true, item, !MainViewModel.SearchInFiles);
                e.Handled = true;
                return;
            }

        }

        private void SearchResults_KeyDown(object sender, KeyEventArgs e)
        {
            var item = (TreeViewEntryItemModel)SearchResults.SelectedItem;
            if (item == null || item.IsBeingEdited)
            {
                return;
            }
            if (e.Key == Key.Delete)
            {
				MainViewModel.TreeViewModel.DeleteTreeItem(item);
                e.Handled = true;
                return;
            }
            if (e.Key == Key.F2)
            {
				MainViewModel.TreeViewModel.StartEditingTreeItem(item);
                e.Handled = true;
                return;
            }
            if (e.Key == Key.Enter)
            {
				MainViewModel.TreeViewModel.OpenItem((TreeViewEntryItemModel)SearchResults.SelectedItem, MainViewModel.SearchText);
                e.Handled = true;
                return;
            }
        }

        private void SearchResults_EndEdit(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewEntryItemModel)SearchResults.SelectedItem;
            if (item == null)
            {
                return;
            }
            var newValue = ((TextBox)sender).Text;
			MainViewModel.TreeViewModel.EndTreeEdit(newValue, true, item, !MainViewModel.SearchInFiles);
        }

        private void SearchResults_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var selectedItem = SearchResults.SelectedItem as TreeViewEntryItemModel;
                if ((selectedItem != null && selectedItem.IsBeingEdited) || IsContextMenuOpened)
                {
                    return;
                }
                var mousePos = e.GetPosition(null);
                var diff = DragStartPoint - mousePos;

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
                if (item != dropTarget)
                {
					MainViewModel.TreeViewModel.MoveTreeItem(item, dropTarget, MainViewModel.WorkspaceDirectoryModel.CurrentWorkspaceDirectory);
                }
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
			IsContextMenuOpened = false;
        }

        private void SearchResults_ContextMenuOpened(object sender, RoutedEventArgs e)
        {
			IsContextMenuOpened = true;
        }

        private void SearchResults_OpenInExplorer(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewEntryItemModel)SearchResults.SelectedItem;
            if (item == null)
            {
                return;
            }
            try
            {
                if (item.NodeType == NodeType.Directory)
                {
                    Process.Start(item.Path);
                }
                else if (item.NodeType == NodeType.File)
                {
                    Process.Start("explorer.exe", "/select, \"" + item.Path + "\"");
                }
                
            }
            catch (Exception ex)
            {
                MessageBoxHelper.ShowError(String.Format("Cannot open path: '{0}' - {1}.", item.Path, ex.Message));
            }
        }

   }
}
