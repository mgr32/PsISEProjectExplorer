using Microsoft.PowerShell.Host.ISE;
using NLog;
using NLog.Config;
using NLog.Targets;
using Ookii.Dialogs.Wpf;
using PsISEProjectExplorer.UI;
using PsISEProjectExplorer.UI.IseIntegration;
using PsISEProjectExplorer.UI.ViewModel;
using System;
using System.Windows;
using System.Windows.Input;

namespace PsISEProjectExplorer
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ProjectExplorerWindow : IAddOnToolHostObject
    {

        private MainViewModel MainViewModel { get; set; }

        private ObjectModelRoot hostObject;

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
            this.DataContext = this.MainViewModel;
            InitializeComponent();
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
                TreeViewEntryItemModel item = this.MainViewModel.TreeViewModel.FindTreeViewEntryItemByPath(path);
                if (item == null)
                {
                    return;
                }

                SearchResults.SelectItem(item);
                this.MainViewModel.IseIntegrator.SetFocusOnCurrentTab();
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
                this.MainViewModel.ChangeRootDirectory(dialog.SelectedPath);
            }
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
            if (e.ClickCount > 1)
            {
                this.MainViewModel.TreeViewModel.SelectItem((TreeViewEntryItemModel)this.SearchResults.SelectedItem, this.MainViewModel.SearchText);
                e.Handled = true;
            }
        }

        private void SearchResults_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.MainViewModel.TreeViewModel.SelectItem((TreeViewEntryItemModel)this.SearchResults.SelectedItem, this.MainViewModel.SearchText);
            }
        }

        private void TextBoxSearchClear_Click(object sender, RoutedEventArgs e)
        {
            this.MainViewModel.SearchText = string.Empty;

        }

   }
}
