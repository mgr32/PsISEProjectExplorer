using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.IO;
using System.Collections.Concurrent;

namespace PsISEProjectExplorer.Config
{
    [Component]
    public class ConfigHandler
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly string ConfigFilePath;

        private Object ConfigHandlerLock = new Object();

        private IDictionary<String, String> cache = new ConcurrentDictionary<string, string>();

        public ConfigHandler()
        {
            ConfigFilePath = Path.Combine(Environment.GetEnvironmentVariable("LOCALAPPDATA"), "PsISEProjectExplorer", "PsISEProjectExplorer.config");
        }

        public string ReadConfigStringValue(string key)
        {
            if (cache.ContainsKey(key))
            {
                return cache[key];
            }
            lock (ConfigHandlerLock)
            {
                var config = OpenConfigFile();
                if (config == null)
                {
                    return null;
                }
                try
                {
                    String value = config.AppSettings.Settings[key].Value;
                    cache[key] = value;
                    return value;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public bool ReadConfigBoolValue(string key, bool defaultValue)
        {
            string value = ReadConfigStringValue(key);
            bool result;
            if (!Boolean.TryParse(value, out result))
            {
                SaveConfigValue(key, defaultValue.ToString());
                return defaultValue;
            }
            return result;
        }

        public int ReadConfigIntValue(string key, int defaultValue)
        {
            string value = ReadConfigStringValue(key);
            int result;
            if (!Int32.TryParse(value, out result))
            {
                SaveConfigValue(key, defaultValue.ToString());
                return defaultValue;
            }
            return result;
        }

        public IEnumerable<string> ReadConfigStringEnumerableValue(string key)
        {
            return ReadConfigStringEnumerableValue(key, false, Enumerable.Empty<string>());
        }

        public IEnumerable<string> ReadConfigStringEnumerableValue(string key, bool toLower, IEnumerable<string> defaultValue)
        {
            var value = ReadConfigStringValue(key);
            if (String.IsNullOrWhiteSpace(value))
            {
                if (defaultValue != null && defaultValue.Any())
                {
                    SaveConfigEnumerableValue(key, defaultValue);
                }
                return defaultValue;
            }
            try
            {
                return toLower ? value.ToLowerInvariant().Split(',') : value.Split(',');
            }
            catch (Exception)
            {
                SaveConfigEnumerableValue(key, defaultValue);
                return defaultValue;
            }
        }

        public void SaveConfigValue(string key, string value)
        {
            lock (ConfigHandlerLock)
            {
                cache[key] = value;
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

        public void SaveConfigEnumerableValue(string key, IEnumerable<string> value)
        {
            var str = value == null ? String.Empty : String.Join(",", value.Where(s => !(String.IsNullOrWhiteSpace(s))));
            SaveConfigValue(key, str);
        }

        public IEnumerable<string> AddConfigEnumerableValue(string key, string value)
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

        public IEnumerable<string> RemoveConfigEnumerableValue(string key, string value)
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

        private Configuration OpenConfigFile()
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
