using NLog;
using System;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace PsISEProjectExplorer.Config
{
    public static class ConfigHandler
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly Configuration Config;

        static ConfigHandler()
        {
            var currentLocation = Assembly.GetExecutingAssembly().Location;
            var currentPath = Path.GetDirectoryName(currentLocation);
            if (currentPath == null)
            {
                return;
            }
            string configFilePath = Path.Combine(currentPath, "PsISEProjectExplorer.config");
            var map = new ExeConfigurationFileMap { ExeConfigFilename = configFilePath };
            try
            {
                Config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            }
            catch (Exception)
            {
                Logger.Error("Cannot open configuration file at " + configFilePath);
            }
        }

        public static string ReadConfigStringValue(string key)
        {
            if (Config == null)
            {
                return null;
            }
            try
            {
                return Config.AppSettings.Settings[key].Value;
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
            if (Config == null)
            {
                return;
            }
            try
            {
                Config.AppSettings.Settings.Remove(key);
                Config.AppSettings.Settings.Add(key, value);
                Config.Save(ConfigurationSaveMode.Modified);
            }
            catch (Exception e)
            {
                Logger.Error("Cannot save config file", e);
            }
        }
    }
}
