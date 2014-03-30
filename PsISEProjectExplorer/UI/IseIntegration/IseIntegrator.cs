using Microsoft.PowerShell.Host.ISE;
using PsISEProjectExplorer.Model;
using System;
using System.ComponentModel;

namespace PsISEProjectExplorer.UI.IseIntegration
{
    public class IseIntegrator
    {
        
        public string SelectedFilePath
        { 
            get
            {
                var file = this.HostObject.CurrentPowerShellTab.Files.SelectedFile;
                return (file == null ? null : file.FullPath);
            }
        }

        public string SelectedText
        {
            get
            {
                var file = this.HostObject.CurrentPowerShellTab.Files.SelectedFile;
                return (file == null ? null : file.Editor.SelectedText);
            }
        }

        public event EventHandler<IseEventArgs> FileTabChanged;

        private ObjectModelRoot HostObject { get; set; }

        public IseIntegrator(ObjectModelRoot hostObject)
        {
            if (hostObject == null)
            {
                throw new ArgumentNullException("hostObject");
            }
  
            this.HostObject = hostObject;
            this.HostObject.CurrentPowerShellTab.PropertyChanged += OnIseTabChanged;
        }

        public void GoToFile(string filePath)
        {
             this.HostObject.CurrentPowerShellTab.Files.Add(filePath);
        }

        public void SetCursor(int line, int column)
        {
            if (this.HostObject.CurrentPowerShellTab.Files.SelectedFile != null)
            {
                this.HostObject.CurrentPowerShellTab.Files.SelectedFile.Editor.SetCaretPosition(line, column);
            }
        }

        public void SelectText(int line, int column, int length)
        {
            if (this.HostObject.CurrentPowerShellTab.Files.SelectedFile != null)
            {
                this.HostObject.CurrentPowerShellTab.Files.SelectedFile.Editor.Select(line, column, line, column + length);
            }
        }

        public EditorInfo GetCurrentLineWithColumnIndex()
        {
            var file = this.HostObject.CurrentPowerShellTab.Files.SelectedFile;
            if (file == null)
            {
                return null;
            }
            return new EditorInfo(file.Editor.CaretLineText, file.Editor.CaretLine, file.Editor.CaretColumn);           
        }

        public void SetFocusOnCurrentTab()
        {
            var file = this.HostObject.CurrentPowerShellTab.Files.SelectedFile;
            if (file == null)
            {
                return;
            }
            file.Editor.Focus();
        }

        private void OnIseTabChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LastEditorWithFocus")
            {
                if (this.FileTabChanged != null)
                {
                    this.FileTabChanged(this, new IseEventArgs()); 
                }
                
            }
        }
    }
}
