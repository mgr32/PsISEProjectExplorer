using PsISEProjectExplorer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.Config
{
    [Component]
    public class ConfigValues
    {
        private static readonly IEnumerable<string> DefaultDslCustomDictionary = new List<string>() {
            "task", // psake
            "serverrole", "serverconnection", "step" // PSCI
        };

        private ConfigHandler configHandler;

        public bool SearchRegex
        {
            get
            {
                return configHandler.ReadConfigBoolValue("SearchRegex", false);
            }
            set
            {
                configHandler.SaveConfigValue("SearchRegex", value.ToString());
            }
        }

        public IndexingMode IndexFilesMode
        {
            get
            {
                String modeStr = configHandler.ReadConfigStringValue("IndexFilesMode");
                if (string.IsNullOrEmpty(modeStr))
                {
                    return IndexingMode.LOCAL_FILES;
                }
                IndexingMode result;
                Enum.TryParse(modeStr, out result);
                return result;
            }
            set
            {
                configHandler.SaveConfigValue("IndexFilesMode", value.ToString());
            }
        }

        public bool ShowAllFiles
        {
            get
            {
                return configHandler.ReadConfigBoolValue("ShowAllFiles", true);
            }
            set
            {
                configHandler.SaveConfigValue("ShowAllFiles", value.ToString());
            }
        }

        public bool SyncWithActiveDocument
        {
            get
            {
                return configHandler.ReadConfigBoolValue("SyncWithActiveDocument", false);
            }
            set
            {
                configHandler.SaveConfigValue("SyncWithActiveDocument", value.ToString());
            }
        }

        public bool AutoUpdateRootDirectory
        {
            get
            {
                return configHandler.ReadConfigBoolValue("AutoUpdateRootDirectory", true);
            }
            set
            {
                configHandler.SaveConfigValue("AutoUpdateRootDirectory", value.ToString());
            }
        }

        public int MaxNumOfWorkspaceDirectories
        {
            get
            {
                return configHandler.ReadConfigIntValue("MaxNumOfWorkspaceDirectories", 5);
            }
        }

        public IEnumerable<string> WorkspaceDirectories
        {
            get
            {
                return configHandler.ReadConfigStringEnumerableValue("WorkspaceDirectories");
            }
            set
            {
                configHandler.SaveConfigEnumerableValue("WorkspaceDirectories", value);
            }
        }

        public bool DslAutoDiscovery
        {
            get
            {
                return configHandler.ReadConfigBoolValue("DslAutoDiscovery", true);
            }
        }

        public IEnumerable<string> ExcludePaths
        {
            get
            {
                return configHandler.ReadConfigStringEnumerableValue("ExcludePaths");
            }
        }

        public bool ParsePowershellDSCWithExternalImports
        {
            get
            {
                return configHandler.ReadConfigBoolValue("ParsePowershellDSCWithExternalImports", false);
            }
        }

        public bool ParsePowershellItems
        {
            get
            {
                return configHandler.ReadConfigBoolValue("ParsePowershellItems", true);
            }
        }

        public IEnumerable<string> DslCustomDictionary
        {
            get
            {
                return configHandler.ReadConfigStringEnumerableValue("DslCustomDictionary", true, DefaultDslCustomDictionary);
            }
        }

        public ConfigValues()
        {
            this.configHandler = new ConfigHandler();
        }

        public IEnumerable<string> AddExcludePath(string path)
        {
            return configHandler.AddConfigEnumerableValue("ExcludePaths", path);
        }

        public IEnumerable<string> RemoveExcludePath(string path)
        {
            return configHandler.RemoveConfigEnumerableValue("ExcludePaths", path);
        }

    }
}
