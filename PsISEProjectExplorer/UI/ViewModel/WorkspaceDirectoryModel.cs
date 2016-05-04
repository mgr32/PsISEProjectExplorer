using PsISEProjectExplorer.Commands;
using PsISEProjectExplorer.Config;
using PsISEProjectExplorer.UI.Helpers;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace PsISEProjectExplorer.UI.ViewModel
{
    [Component]
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
                this.commandExecutor.ExecuteWithParam<ResetWorkspaceDirectoryCommand, bool>(false);
                this.configValues.AutoUpdateRootDirectory = value;
            }
        }

        private readonly int maxNumOfWorkspaceDirectories;

        private readonly ConfigValues configValues;

        private readonly CommandExecutor commandExecutor;

        private readonly MessageBoxHelper messageBoxHelper;

        public WorkspaceDirectoryModel(ConfigValues configValues, CommandExecutor commandExecutor, MessageBoxHelper messageBoxHelper)
        {
            this.configValues = configValues;
            this.commandExecutor = commandExecutor;
            this.messageBoxHelper = messageBoxHelper;

            this.maxNumOfWorkspaceDirectories = configValues.MaxNumOfWorkspaceDirectories;
            this.WorkspaceDirectories = new ObservableCollection<string>(configValues.WorkspaceDirectories);
            this.autoUpdateRootDirectory = configValues.AutoUpdateRootDirectory;

            this.SanitizeWorkspaceDirectories();
        }

        public void TriggerWorkspaceDirectoryChange()
        {
            this.OnPropertyChanged("WorkspaceDirectories");
            this.OnPropertyChanged("CurrentWorkspaceDirectory");
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
                messageBoxHelper.ShowError(String.Format("Directory {0} does not exist.", path));
                this.TriggerWorkspaceDirectoryChange();
                return;
            }
            this.WorkspaceDirectories.Insert(0, path);
            var cnt = this.WorkspaceDirectories.Count;
            while (cnt > this.maxNumOfWorkspaceDirectories)
            {
                this.WorkspaceDirectories.RemoveAt(cnt - 1);
                cnt = this.WorkspaceDirectories.Count;
            }

            this.configValues.WorkspaceDirectories = this.WorkspaceDirectories;

            this.TriggerWorkspaceDirectoryChange();
        }

        private void SanitizeWorkspaceDirectories()
        {
            var itemsToRemove = this.WorkspaceDirectories.Where(wd => !Directory.Exists(wd)).ToList();
            foreach (var item in itemsToRemove)
            {
                this.WorkspaceDirectories.Remove(item);
            }
        }

    }
}
