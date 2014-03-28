using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PsISEProjectExplorer.Config
{
    public class ConfigHandler
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static Configuration config;

        static ConfigHandler()
        {
            string configFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "PsISEProjectExplorer.config");
            ExeConfigurationFileMap map = new ExeConfigurationFileMap { ExeConfigFilename = configFilePath };
            try
            {
                config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            }
            catch (Exception)
            {
                logger.Error("Cannot open configuration file at " + configFilePath);
            }
        }

        public static string ReadConfigStringValue(string key)
        {
            if (config == null)
            {
                return null;
            }
            try
            {
                return config.AppSettings.Settings[key].Value;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool ReadConfigBoolValue(string key)
        {
            string value = ReadConfigStringValue(key);
            bool result;
            Boolean.TryParse(value, out result);
            return result;
        }

        public static void SaveConfigValue(string key, string value)
        {
            if (config == null)
            {
                return;
            }
            try
            {
                config.AppSettings.Settings.Remove(key);
                config.AppSettings.Settings.Add(key, value);
                config.Save(ConfigurationSaveMode.Modified);
            }
            catch (Exception)
            {
                return;
            }
        }
    }
}
