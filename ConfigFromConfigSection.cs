using System;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace Aurora.Configs
{           
    public class ConfigFromConfigSection : Config 
    {                                                  
        #region Properties
        public CustomConfigSection ConfigSection { get; private set; }
        public Configuration Configuration { get; private set; }
        #endregion
        #region To life and die in starlight

        public ConfigFromConfigSection(ConfigType type, string configLocation) : base(type, configLocation)
        {
            LoadConfigurationFile();
        }

        public ConfigFromConfigSection(ConfigType type, string configLocation, string configFile) : base(type, configLocation, configFile)
        {
            LoadConfigurationFile();
        }
        #endregion             
        #region Private Members              
        
        //private string m_SectionName;
        #endregion
        #region Public Methods          
        public CustomConfigSection LoadSection(string sectionName)
        {
            try
            {
                ConfigSection = Configuration.GetSection(sectionName) as CustomConfigSection;
                if (ConfigSection == null)
                {
                    Configuration.Sections.Add(sectionName, new CustomConfigSection());
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error reading config {0}", ex);
            }
            return (ConfigSection);
        }                                              
        public void SaveSection(string sectionName)
        {
            try
            {
                ConfigSection.LockItem = false;
                Configuration.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(sectionName);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error saving config {0}", ex);
            }
        }
        public void UpdateConfig(string sectionName, string parameter, object value, bool doSave = true)
        {
            if (ConfigSection == null)
                return;
            
            PropertyInfo info = ConfigSection.GetType().GetProperty(parameter, BindingFlags.Public | BindingFlags.Instance);
            if (info == null)
            {
                Log.Error("Could not update configuration for {0} ", parameter);
                return;
            }

            object currentValue = info.GetValue(ConfigSection, null);
            if (currentValue == null || !currentValue.Equals(value))
            {
                info.SetValue(ConfigSection, value);
                if (doSave)
                    SaveSection(sectionName);
            }
        }
        #endregion

        #region Private Methods
        private void LoadConfigurationFile()                                                                           
        {
            if (File.Exists(ConfigFilePath))
            {
                ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap
                {
                    ExeConfigFilename = System.Environment.ExpandEnvironmentVariables(ConfigFilePath)
                };
                Configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            }
        }             
        #endregion
    }
}
