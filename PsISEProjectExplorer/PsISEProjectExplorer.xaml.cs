using Microsoft.PowerShell.Host.ISE;
using PsISEProjectExplorer.UI.IseIntegration;
using PsISEProjectExplorer.UI.ViewModel;
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace PsISEProjectExplorer
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ProjectExplorerWindow : UserControl, IAddOnToolHostObject
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
            this.MainViewModel = new MainViewModel();
            this.DataContext = this.MainViewModel;
            InitializeComponent();
        }

        public void GoToDefinition()
        {
            this.MainViewModel.GoToDefinition();
        }

        public void FindAllOccurrences()
        {
            this.MainViewModel.FindAllOccurrences();
        }

        private void SearchResults_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
            {
                this.MainViewModel.TreeViewModel.SelectItem((TreeViewEntryItemModel)this.SearchResults.SelectedItem);
                e.Handled = true;
            }
        }

        private void SearchResults_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.MainViewModel.TreeViewModel.SelectItem((TreeViewEntryItemModel)this.SearchResults.SelectedItem);
            }
        }


       
    }
}
