using Microsoft.PowerShell.Host.ISE;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.IseIntegration
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
