using Microsoft.PowerShell.Host.ISE;
using NLog;
using PsISEProjectExplorer.Model;
using PsISEProjectExplorer.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace PsISEProjectExplorer.UI.IseIntegration
{
    public class IseIntegrator
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
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

        public IEnumerable<ISEFile> OpenIseFiles
        {
            get
            {
                var files = this.HostObject.CurrentPowerShellTab.Files;
                return files == null ? new List<ISEFile>() : files.ToList();
            }
        }

        public IEnumerable<string> OpenFiles
        {
            get
            {
                return this.OpenIseFiles.Select(f => f.FullPath).ToList();
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
            try
            {
                this.HostObject.CurrentPowerShellTab.Files.Add(filePath);
            }
            catch (Exception e)
            {
                MessageBoxHelper.ShowError(String.Format("Cannot open file due to Powershell ISE error: '{0}'", e.Message));
            }
        }

        public void SetCursor(int line, int column)
        {
            if (this.HostObject.CurrentPowerShellTab.Files.SelectedFile != null)
            {
                var editor = this.HostObject.CurrentPowerShellTab.Files.SelectedFile.Editor;
                if (editor.LineCount > line)
                {
                    try
                    {
                        editor.SetCaretPosition(line, column);
                    }
                    catch (Exception e) 
                    {
                        Logger.Error("Failed to set cursor", e);
                    }
                }
            }
        }

        public void SelectText(int line, int column, int length)
        {
            if (this.HostObject.CurrentPowerShellTab.Files.SelectedFile != null)
            {
                var editor = this.HostObject.CurrentPowerShellTab.Files.SelectedFile.Editor;
                if (editor.LineCount > line)
                {
                    try
                    {
                        editor.Select(line, column, line, column + length);
                    }
                    catch (Exception e)
                    {
                        Logger.Error("Failed to select text", e);
                    }
                }
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
                    try
                    {
                        this.HostObject.CurrentPowerShellTab.Files.Remove(file);
                    }
                    catch
                    {
                        // ignore -> can be unsaved
                    }
                }
            }
        }

        public bool CloseFile(string path)
        {
            var file = this.GetIseFile(path);
            if (file != null)
            {
                try
                {
                    return this.HostObject.CurrentPowerShellTab.Files.Remove(file);
                }
                catch (Exception e)
                {
                    MessageBoxHelper.ShowError(String.Format("Cannot close file '{0}': {1}", file.FullPath, e.Message));
                }
            }
            return false;
        }

        public bool IsFileSaved(string path)
        {
            var file = this.GetIseFile(path);
            return (file != null && file.IsSaved);
        }

        public void AttachFileCollectionChangedHandler(NotifyCollectionChangedEventHandler handler)
        {
            this.HostObject.CurrentPowerShellTab.Files.CollectionChanged += handler;
            var openFiles = this.HostObject.CurrentPowerShellTab.Files;
            if (openFiles.Any())
            {
                handler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, openFiles));
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

        private ISEFile GetIseFile(string path)
        {
            if (this.HostObject.CurrentPowerShellTab == null || this.HostObject.CurrentPowerShellTab.Files == null)
            {
                return null;
            }
            return this.HostObject.CurrentPowerShellTab.Files.FirstOrDefault(f => f.FullPath.Equals(path, StringComparison.InvariantCultureIgnoreCase));
        }

    }
}
