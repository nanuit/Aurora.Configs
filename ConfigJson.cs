using System;
using System.IO;
using System.Reflection;
using ServiceStack.Text;


namespace Aurora.Configs
{           
    public class ConfigJson<T> : Config where T : new()
    {                                                  
        #region Properties
        public T Configuration { get; set; }
        #endregion
        #region To life and die in starlight
        public ConfigJson(ConfigType type, string configLocation) : base(type, configLocation) { }
        public ConfigJson(ConfigType type, string configLocation, string configFile) : base(type, configLocation, configFile) { }
        #endregion
        #region Public Methods          
        public T Load()
        {
            try
            {   
                if (File.Exists(ConfigFilePath))
                {
                    string config = File.ReadAllText(ConfigFilePath);
                    Configuration = ServiceStack.Text.JsonSerializer.DeserializeFromString<T>(config);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error reading config {0}", ex);
            }
            if (Configuration == null)
                Configuration = new T();
            return (Configuration);
        }
        public T Load(ConfigType type, string configLocation, string configFilename = null)
        {              
            try
            {                                                                                                                        
                SetConfigFile(type, configLocation, !string.IsNullOrEmpty(configFilename) ? configFilename : $"{typeof(T).Name}.json");
                Configuration = Load();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error reading config {0}", ex);
            }
            return (Configuration);
        }

        public void Save()
        {
            try
            {
                using (var stream = File.CreateText(new Uri(ConfigFilePath).LocalPath))
                {
                    string json = JsonSerializer.SerializeToString<T>(Configuration);
                    json = json.IndentJson();
                    stream.Write(json);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error saving config {0}", ex);
            }
        }
        public void Save(ConfigType type, string configLocation, string configFilename = null)
        {
            try
            {
                SetConfigFile(type, configLocation, !string.IsNullOrEmpty(configFilename) ? configFilename : $"{typeof(T).Name}.json");
                Save();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error saving config {0}", ex);
            }          
        }

        public void UpdateConfig(string parameter, object value, bool doSave = true)
        {
            if (Configuration == null)
                return;
            
            PropertyInfo info = Configuration.GetType().GetProperty(parameter, BindingFlags.Public | BindingFlags.Instance);
            if (info == null)
            {
                Log.Error("Could not update configuration for {0} ", parameter);
                return;
            }

            object currentValue = info.GetValue(Configuration, null);
            if (currentValue == null || !currentValue.Equals(value))
            {
                info.SetValue(Configuration, value);
                if (doSave)
                    Save();
            }
        }
        #endregion
    }
}
