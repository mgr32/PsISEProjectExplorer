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
                return WorkspaceDirectories.FirstOrDefault();
            }

            set
            {
                if (!String.IsNullOrEmpty(value))
                {
					SetWorkspaceDirectory(value);
                }
            }
        }

        public ObservableCollection<string> WorkspaceDirectories { get; private set; }

        private bool autoUpdateRootDirectory;

        public bool AutoUpdateRootDirectory
        {
            get 
            { 
                return autoUpdateRootDirectory; 
            }
            set
            {
				autoUpdateRootDirectory = value;
				OnPropertyChanged();
				ResetWorkspaceDirectoryIfRequired();
                ConfigHandler.SaveConfigValue("AutoUpdateRootDirectory", value.ToString());
            }
        }

        private int MaxNumOfWorkspaceDirectories { get; set; }

        public IseIntegrator IseIntegrator { get; set; }

        public WorkspaceDirectoryModel()
        {
			MaxNumOfWorkspaceDirectories = ConfigHandler.ReadConfigIntValue("MaxNumOfWorkspaceDirectories", 5);
            var workspaceDirs = ConfigHandler.ReadConfigStringEnumerableValue("WorkspaceDirectories");
			WorkspaceDirectories = new ObservableCollection<string>(workspaceDirs);
			autoUpdateRootDirectory = ConfigHandler.ReadConfigBoolValue("AutoUpdateRootDirectory", true);

            // handle old config value -> to be removed in future
            var oldRootDirectoryToSearch = ConfigHandler.ReadConfigStringValue("RootDirectory");
            if (!WorkspaceDirectories.Any() && !String.IsNullOrEmpty(oldRootDirectoryToSearch))
            {
				WorkspaceDirectories = new ObservableCollection<string>(new List<string>() { oldRootDirectoryToSearch });
            }
			// ~

			SanitizeWorkspaceDirectories();
        }

        public void SetWorkspaceDirectory(string path)
        {
            var posInList = WorkspaceDirectories.IndexOf(path);

            if (posInList != -1)
            {
                // first already - no change
                if (posInList == 0)
                {
                    return;
                }
				WorkspaceDirectories.RemoveAt(posInList);
            }
            if (!Directory.Exists(path))
            {
                MessageBoxHelper.ShowError(String.Format("Directory {0} does not exist.", path));
				OnPropertyChanged("WorkspaceDirectories");
				OnPropertyChanged("CurrentWorkspaceDirectory");
                return;
            }
			WorkspaceDirectories.Insert(0, path);
            var cnt = WorkspaceDirectories.Count;
            while (cnt > MaxNumOfWorkspaceDirectories)
            {
				WorkspaceDirectories.RemoveAt(cnt - 1);
                cnt = WorkspaceDirectories.Count;
            }

            ConfigHandler.SaveConfigEnumerableValue("WorkspaceDirectories", WorkspaceDirectories);

			OnPropertyChanged("WorkspaceDirectories");
			OnPropertyChanged("CurrentWorkspaceDirectory");
        }

        private void SanitizeWorkspaceDirectories()
        {
            var itemsToRemove = WorkspaceDirectories.Where(wd => !Directory.Exists(wd)).ToList();
            foreach (var item in itemsToRemove)
            {
				WorkspaceDirectories.Remove(item);
            }
        }

        public bool ResetWorkspaceDirectoryIfRequired()
        {
            if (IseIntegrator == null)
            {
                return false;
            }
            var currentPath = IseIntegrator.SelectedFilePath;
            if (String.IsNullOrEmpty(currentPath) || currentPath == CurrentWorkspaceDirectory)
            {
                return false;
            }
            if (!AutoUpdateRootDirectory && CurrentWorkspaceDirectory != null)
            {
                return false;
            }
            string newRootDirectoryToSearch = RootDirectoryProvider.GetRootDirectoryToSearch(currentPath);
            if (newRootDirectoryToSearch == null || newRootDirectoryToSearch == CurrentWorkspaceDirectory || 
                FileSystemOperationsService.IsSubdirectory(CurrentWorkspaceDirectory, newRootDirectoryToSearch) ||
                !Directory.Exists(newRootDirectoryToSearch))
            {
                return false;
            }
			SetWorkspaceDirectory(newRootDirectoryToSearch);
            return true;
        }

    }
}
