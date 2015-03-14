using NLog;
using System;
using System.Collections.Generic;
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
            catch (Exception e)
            {
                Logger.Error("Cannot open configuration file at " + configFilePath, e);
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

        public static bool ReadConfigBoolValue(string key, bool defaultValue)
        {
            string value = ReadConfigStringValue(key);
            bool result;
            if (!Boolean.TryParse(value, out result))
            {
                return defaultValue;
            }
            return result;
        }

        public static int ReadConfigIntValue(string key, int defaultValue)
        {
            string value = ReadConfigStringValue(key);
            int result;
            if (!Int32.TryParse(value, out result))
            {
                return defaultValue;
            }
            return result;
        }

        public static IEnumerable<string> ReadConfigStringEnumerableValue(string key)
        {
            var value = ReadConfigStringValue(key);
            if (value == null)
            {
                return new string[0];
            }
            try
            {
                return value.Split(',');
            }
            catch (Exception)
            {
                return new string[0];
            }
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

        public static void SaveConfigEnumerableValue(string key, IEnumerable<string> value)
        {
            var str = value == null ? String.Empty : String.Join(",", value);
            SaveConfigValue(key, str);
        }
    }
}
