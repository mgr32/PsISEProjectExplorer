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
    [Component]
    public class IseIntegrator
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        public string SelectedFilePath
        { 
            get
            {
                var file = this.hostObject.CurrentPowerShellTab.Files.SelectedFile;
                return (file == null ? null : file.FullPath);
            }
        }

        public string SelectedText
        {
            get
            {
                var file = this.hostObject.CurrentPowerShellTab.Files.SelectedFile;
                return (file == null ? null : file.Editor.SelectedText);
            }
        }

        public IEnumerable<ISEFile> OpenIseFiles
        {
            get
            {
                var files = this.hostObject.CurrentPowerShellTab.Files;
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

        private ObjectModelRoot hostObject;

        private string currentSelectedFile;

        private readonly MessageBoxHelper messageBoxHelper;

        public IseIntegrator(MessageBoxHelper messageBoxHelper)
        {
            this.messageBoxHelper = messageBoxHelper;
        }

        public void setHostObject(ObjectModelRoot hostObject)
        {
            if (hostObject == null)
            {
                throw new ArgumentNullException("hostObject");
            }
  
            this.hostObject = hostObject;
            this.hostObject.CurrentPowerShellTab.PropertyChanged += OnIseTabChanged;
        }

        public void GoToFile(string filePath)
        {
            try
            {
                Logger.Debug("ISEIntegrator - opening file " + filePath);
                this.hostObject.CurrentPowerShellTab.Files.Add(filePath);
            }
            catch (Exception e)
            {
                this.messageBoxHelper.ShowError(String.Format("Cannot open file due to Powershell ISE error: '{0}'", e.Message));
            }
        }

        public void SetCursor(int line, int column)
        {
            if (this.hostObject.CurrentPowerShellTab.Files.SelectedFile == null)
            {
                return;
            }
            Logger.Debug("ISEIntegrator - setting cursor to line " + line + ", column " + column);
            var editor = this.hostObject.CurrentPowerShellTab.Files.SelectedFile.Editor;
            if (editor.LineCount <= line)
            {
                return;
            }
            try
            {
                editor.SetCaretPosition(line, column);
            }
            catch (Exception e) 
            {
                Logger.Error(e, "Failed to set cursor");
            }            
        }

        public void SelectText(int line, int column, int length)
        {
            if (this.hostObject.CurrentPowerShellTab.Files.SelectedFile == null)
            {
                return;
            }
            Logger.Debug("IseIntegrator - selecting text at line " + line + ", column " + column + ", length " + length);
            var editor = this.hostObject.CurrentPowerShellTab.Files.SelectedFile.Editor;
            if (editor.LineCount <= line)
            {
                return;
            }
            try
            {
                editor.Select(line, column, line, column + length);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to select text");
            }
        }

        public void WriteTextWithNewLine(String text)
        {
            if (this.hostObject.CurrentPowerShellTab.Files.SelectedFile == null)
            {
                return;
            }
            var editor = this.hostObject.CurrentPowerShellTab.Files.SelectedFile.Editor;
            if (editor.CaretColumn > 1)
            {
                editor.SetCaretPosition(editor.CaretLine, 1);
            }
            editor.InsertText(text + Environment.NewLine);
        }

        public EditorInfo GetCurrentLineWithColumnIndex()
        {
            var file = this.hostObject.CurrentPowerShellTab.Files.SelectedFile;
            if (file == null)
            {
                return null;
            }
            return new EditorInfo(file.Editor.CaretLineText, file.Editor.CaretLine, file.Editor.CaretColumn);           
        }

        public void SetFocusOnCurrentTab()
        {
            Logger.Debug("IseIntegrator - setting focus on current tab");
            var file = this.hostObject.CurrentPowerShellTab.Files.SelectedFile;
            if (file == null)
            {
                return;
            }
            file.Editor.Focus();
        }

        public void CloseAllButThis()
        {
            if (this.hostObject.CurrentPowerShellTab == null || this.hostObject.CurrentPowerShellTab.Files == null)
            {
                return;
            }
            Logger.Debug("IseIntegrator - closing all but this");
            var filesToRemove = new List<ISEFile>(this.hostObject.CurrentPowerShellTab.Files);
            var selectedFile = this.hostObject.CurrentPowerShellTab.Files.SelectedFile;
            foreach (var file in filesToRemove)
            {
                if (file != selectedFile)
                {
                    try
                    {
                        this.hostObject.CurrentPowerShellTab.Files.Remove(file);
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
            Logger.Debug("IseIntegrator - closing file " + path);
            var file = this.GetIseFile(path);
            if (file != null)
            {
                try
                {
                    return this.hostObject.CurrentPowerShellTab.Files.Remove(file);
                }
                catch (Exception e)
                {
                    this.messageBoxHelper.ShowError(String.Format("Cannot close file '{0}': {1}", file.FullPath, e.Message));
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
            this.hostObject.CurrentPowerShellTab.Files.CollectionChanged += handler;
            var openFiles = this.hostObject.CurrentPowerShellTab.Files;
            if (openFiles.Any())
            {
                handler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, openFiles));
            }
        }

        private void OnIseTabChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "LastEditorWithFocus" || this.SelectedFilePath == this.currentSelectedFile)
            {
                return;
            }
            this.currentSelectedFile = this.SelectedFilePath;
            if (this.FileTabChanged != null)
            {
                this.FileTabChanged(this, new IseEventArgs()); 
            }
        }

        private ISEFile GetIseFile(string path)
        {
            if (this.hostObject.CurrentPowerShellTab == null || this.hostObject.CurrentPowerShellTab.Files == null)
            {
                return null;
            }
            return this.hostObject.CurrentPowerShellTab.Files.FirstOrDefault(f => f.FullPath.Equals(path, StringComparison.InvariantCultureIgnoreCase));
        }

    }
}
