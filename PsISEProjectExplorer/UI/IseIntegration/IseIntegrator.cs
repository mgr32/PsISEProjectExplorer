using Microsoft.PowerShell.Host.ISE;
using PsISEProjectExplorer.Model;
using System;
using System.Collections.Generic;
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

        private string CurrentSelectedFile { get; set; }

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

        public void ReopenFileAfterRename(string oldPath, string newPath)
        {
            ISEFile file = this.FindFile(oldPath);
            if (file != null) 
            {
                file.SaveAs(newPath);
            }
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

        public void CloseAllButThis()
        {
            if (this.HostObject.CurrentPowerShellTab == null || this.HostObject.CurrentPowerShellTab.Files == null)
            {
                return;
            }
            var filesToRemove = new List<ISEFile>(this.HostObject.CurrentPowerShellTab.Files);
            var selectedFile = this.HostObject.CurrentPowerShellTab.Files.SelectedFile;
            foreach (var file in filesToRemove)
            {
                if (file != selectedFile)
                {
                    this.HostObject.CurrentPowerShellTab.Files.Remove(file);
                }
            }
        }

        private void OnIseTabChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LastEditorWithFocus")
            {
                if (this.SelectedFilePath != this.CurrentSelectedFile) {
                    this.CurrentSelectedFile = this.SelectedFilePath;
                    if (this.FileTabChanged != null)
                    {
                        this.FileTabChanged(this, new IseEventArgs()); 
                    }
                }
                
            }
        }

        private ISEFile FindFile(string path)
        {
            foreach (ISEFile file in this.HostObject.CurrentPowerShellTab.Files)
            {
                if (file.FullPath.Equals(path, StringComparison.InvariantCultureIgnoreCase))
                {
                    return file;
                }
            }
            return null;
        }
    }
}
