using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using Aurora.Crypt;
using NLog;

namespace Aurora.Configs
{
    public abstract class CustomConfig
    {                             
        #region Properties
        public string ConfigFileToUse { get; set; }
        #endregion
        #region Abstract Members 
        /// <summary>
        /// Encryption application identifier
        /// </summary>
        /// <returns>application Identifier for encryption</returns>
        protected abstract string AppIdentifier();
        /// <summary>
        /// specifies the config file containing the Configuration. If ommited one of the other methods to identify the config file is used
        /// </summary>
        /// <returns></returns>
        protected abstract string FullConfigFile();
        /// <summary>
        /// Name of the Environment variable describing the config file. if 
        /// </summary>
        /// <returns></returns>
        protected abstract string EnvironmentVariable();
        /// <summary>
        /// name of the config file residing in the execution path of the assembly. If null or empty the name of the Executing assembly with ".config" extension is used
        /// </summary>
        /// <returns></returns>
        protected abstract string ConfigFileName();                               
        #endregion
        #region To Life and Die in Starlight                 
        public CustomConfig()
        {
            Config = LoadConfiguration();
        }
        #endregion
        #region Private Members
        protected readonly Logger Log = LogManager.GetCurrentClassLogger();
        protected Configuration Config;
        #endregion
        #region Public Methods
        public string DecodeCredentials(string cipher)
        {
            return (RijndaelCrypt.Decrypt(cipher, AppIdentifier()));
        }
        public string EncodeCredentials(string userId, string password)
        {
            string encoded = string.Empty;
            encoded = RijndaelCrypt.Encrypt(password, AppIdentifier());
            return (encoded);
        }
        #endregion
        #region Private Methods
        private Configuration LoadConfiguration()
        {
            try
            {
                string fullConfigFile = FullConfigFile();
                if (!string.IsNullOrEmpty(fullConfigFile))
                {
                    if (File.Exists(fullConfigFile))
                        Log.Warn("Configfile from Code specified {0}", fullConfigFile);
                    else
                    {
                        Log.Error("Logfile in Code specified, but does not exist {0}", fullConfigFile);
                        fullConfigFile = string.Empty;
                    }                                
                }
                if (string.IsNullOrEmpty(fullConfigFile))
                {
                    if (!string.IsNullOrEmpty(EnvironmentVariable()))
                    {
                        fullConfigFile = Environment.ExpandEnvironmentVariables(EnvironmentVariable());
                        if (File.Exists(fullConfigFile))
                        {
                            Log.Warn("Configfile from Environment {0}", fullConfigFile);
                        }
                        else
                        {
                            Log.Warn("Configfile from Environment {0} but does not exist", fullConfigFile);
                            fullConfigFile = string.Empty;
                        }
                    }
                }
                if (string.IsNullOrEmpty(fullConfigFile))
                {

                    if (string.IsNullOrEmpty(ConfigFileName()))
                        fullConfigFile = new Uri(Assembly.GetEntryAssembly().GetName().CodeBase).LocalPath + ".config";
                    else
                        fullConfigFile = Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().GetName().CodeBase).LocalPath), ConfigFileName());
                    if (File.Exists(fullConfigFile))
                    {
                        Log.Warn("Configfile from Executionpath {0}", fullConfigFile);
                    }
                    else
                    {
                        Log.Warn("Configfile from Executionpath  {0} but does not exist", fullConfigFile);
                    }

                }

                if (!File.Exists(fullConfigFile))
                {
                    throw (new Exception($"{fullConfigFile} file not found"));
                }
                ConfigFileToUse = fullConfigFile;
                ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap
                {
                    ExeConfigFilename = System.Environment.ExpandEnvironmentVariables(fullConfigFile)
                };
                return (ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading the configuration: {0}", ex.ToString());
                throw (new Exception("Config File not found", ex));
            }                  
        }
        #endregion
    }
}
