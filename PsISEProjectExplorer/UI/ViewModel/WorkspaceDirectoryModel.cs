using PsISEProjectExplorer.Config;
using PsISEProjectExplorer.Services;
using PsISEProjectExplorer.UI.Helpers;
using PsISEProjectExplorer.UI.IseIntegration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace PsISEProjectExplorer.UI.ViewModel
{
    public class WorkspaceDirectoryModel : BaseViewModel
    {
        public string CurrentWorkspaceDirectory
        {
            get
            {
                return this.WorkspaceDirectories.FirstOrDefault();
            }

            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    this.SetWorkspaceDirectory(value);
                }
            }
        }

        public ObservableCollection<string> WorkspaceDirectories { get; private set; }

        private bool autoUpdateRootDirectory;

        public bool AutoUpdateRootDirectory
        {
            get
            {
                return this.autoUpdateRootDirectory;
            }
            set
            {
                this.autoUpdateRootDirectory = value;
                this.OnPropertyChanged();
                this.ResetWorkspaceDirectoryIfRequired();
                ConfigHandler.SaveConfigValue("AutoUpdateRootDirectory", value.ToString());
            }
        }

        private int MaxNumOfWorkspaceDirectories { get; set; }

        public IseIntegrator IseIntegrator { get; set; }

        public WorkspaceDirectoryModel()
        {
            this.MaxNumOfWorkspaceDirectories = ConfigHandler.ReadConfigIntValue("MaxNumOfWorkspaceDirectories", 5);
            var workspaceDirs = ConfigHandler.ReadConfigStringEnumerableValue("WorkspaceDirectories");
            this.WorkspaceDirectories = new ObservableCollection<string>(workspaceDirs);
            this.autoUpdateRootDirectory = ConfigHandler.ReadConfigBoolValue("AutoUpdateRootDirectory", true);

            this.SanitizeWorkspaceDirectories();
        }

        public void SetWorkspaceDirectory(string path)
        {
            var posInList = this.WorkspaceDirectories.IndexOf(path);

            if (posInList != -1)
            {
                // first already - no change
                if (posInList == 0)
                {
                    return;
                }
                this.WorkspaceDirectories.RemoveAt(posInList);
            }
            if (!Directory.Exists(path))
            {
                MessageBoxHelper.ShowError(String.Format("Directory {0} does not exist.", path));
                this.OnPropertyChanged("WorkspaceDirectories");
                this.OnPropertyChanged("CurrentWorkspaceDirectory");
                return;
            }
            this.WorkspaceDirectories.Insert(0, path);
            var cnt = this.WorkspaceDirectories.Count;
            while (cnt > this.MaxNumOfWorkspaceDirectories)
            {
                this.WorkspaceDirectories.RemoveAt(cnt - 1);
                cnt = this.WorkspaceDirectories.Count;
            }

            ConfigHandler.SaveConfigEnumerableValue("WorkspaceDirectories", this.WorkspaceDirectories);

            this.OnPropertyChanged("WorkspaceDirectories");
            this.OnPropertyChanged("CurrentWorkspaceDirectory");
        }

        private void SanitizeWorkspaceDirectories()
        {
            var itemsToRemove = this.WorkspaceDirectories.Where(wd => !Directory.Exists(wd)).ToList();
            foreach (var item in itemsToRemove)
            {
                this.WorkspaceDirectories.Remove(item);
            }
        }

        public bool ResetWorkspaceDirectoryIfRequired()
        {
            if (this.IseIntegrator == null)
            {
                return false;
            }
            var currentPath = this.IseIntegrator.SelectedFilePath;
            if (String.IsNullOrEmpty(currentPath) || currentPath == this.CurrentWorkspaceDirectory)
            {
                return false;
            }
            if (!this.AutoUpdateRootDirectory && this.CurrentWorkspaceDirectory != null)
            {
                return false;
            }
            string newRootDirectoryToSearch = RootDirectoryProvider.GetRootDirectoryToSearch(currentPath);
            if (newRootDirectoryToSearch == null || newRootDirectoryToSearch == this.CurrentWorkspaceDirectory || 
                FileSystemOperationsService.IsSubdirectory(this.CurrentWorkspaceDirectory, newRootDirectoryToSearch) ||
                !Directory.Exists(newRootDirectoryToSearch))
            {
                return false;
            }
            this.SetWorkspaceDirectory(newRootDirectoryToSearch);
            return true;
        }

    }
}
