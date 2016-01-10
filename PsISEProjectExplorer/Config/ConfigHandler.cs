using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.IO;

namespace PsISEProjectExplorer.Config
{
    public static class ConfigHandler
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly string ConfigFilePath;

        private static Object ConfigHandlerLock = new Object();

        static ConfigHandler()
        {
            ConfigFilePath = Path.Combine(Environment.GetEnvironmentVariable("LOCALAPPDATA"), "PsISEProjectExplorer", "PsISEProjectExplorer.config");
        }

        public static string ReadConfigStringValue(string key)
        {
            lock (ConfigHandlerLock)
            {
                var config = OpenConfigFile();
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
            if (String.IsNullOrWhiteSpace(value))
            {
                return Enumerable.Empty<string>();
            }
            try
            {
                return value.Split(',');
            }
            catch (Exception)
            {
                return Enumerable.Empty<string>();
            }
        }

        public static void SaveConfigValue(string key, string value)
        {
            lock (ConfigHandlerLock)
            {
                var config = OpenConfigFile();
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
                catch (Exception e)
                {
                    Logger.Error(e, "Cannot save config file");
                }
            }
        }

        public static void SaveConfigEnumerableValue(string key, IEnumerable<string> value)
        {
            var str = value == null ? String.Empty : String.Join(",", value.Where(s => !(String.IsNullOrWhiteSpace(s))));
            SaveConfigValue(key, str);
        }

        public static IEnumerable<string> AddConfigEnumerableValue(string key, string value)
        { 
            lock (ConfigHandlerLock)
            {
                IEnumerable<string> currentValue = ReadConfigStringEnumerableValue(key);
                if (!currentValue.Contains(value))
                {
                    currentValue = currentValue.ToList();
                    ((IList<string>)currentValue).Add(value);
                    SaveConfigEnumerableValue(key, currentValue);
                }
                return currentValue;
            }
        }

        public static IEnumerable<string> RemoveConfigEnumerableValue(string key, string value)
        {
            lock (ConfigHandlerLock)
            {
                IEnumerable<string> currentValue = ReadConfigStringEnumerableValue(key);
                if (currentValue.Contains(value))
                {
                    currentValue = currentValue.ToList();
                    ((IList<string>)currentValue).Remove(value);
                    SaveConfigEnumerableValue(key, currentValue);
                }
                return currentValue;
            }
        }

        private static Configuration OpenConfigFile()
        {
            var map = new ExeConfigurationFileMap { ExeConfigFilename = ConfigFilePath };
            try
            {
                return ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Cannot open configuration file at " + ConfigFilePath);
                return null;
            }
        }
    }
}
