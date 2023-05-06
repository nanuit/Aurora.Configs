using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using Aurora.Crypt;
using NLog;

namespace Aurora.Configs
{
    /// <summary>
    /// indicates where the config file should be stored
    /// </summary>
    public enum ConfigType
    {
        /// <summary>
        /// store the config file in the roaming user profile 
        /// </summary>
        RoamingProfile,
        /// <summary>
        /// store the config file in the local user profile
        /// </summary>
        LocalProfile,
        /// <summary>
        /// store the config file in the profile for all users
        /// </summary>
        CommonProfile,
        /// <summary>
        /// store the config file in the path of the executing assembly
        /// </summary>
        Executeable,
        /// <summary>
        /// store the config file in the path specified with the constructor in configName Parameter
        /// </summary>
        Specific,
        /// <summary>
        /// get the config filename from the specified einvornmentvariables
        /// </summary>
        EnvironmentVariable,
        /// <summary>
        /// get the location of the config file in the following order if the parameter is specified and file exists at that location -> specified config file location, Environment,  executeable location with Configfilename
        /// </summary>
        Dynamic
    }
    /// <summary>
    /// handling config files in windows XML format
    /// </summary>
    public class Config
    {            
        #region Static Members
        /// <summary>
        /// nlog instance
        /// </summary>
        protected readonly Logger Log = LogManager.GetCurrentClassLogger();
        #endregion
        #region Private Members
        /// <summary>
        /// used config file path
        /// </summary>
        protected string ConfigFilePath;
        #endregion

        #region Properties

        /// <summary>
        /// Encryption application identifier
        /// </summary>
        /// <returns>application Identifier for encryption</returns>
        public static string AppIdentifier { get; set; }

        /// <summary>
        /// specifies the config file containing the Configuration. If omited one of the other methods to identify the config file is used
        /// </summary>
        /// <returns></returns>
        public string FullConfigFile { get; set; }
        /// <summary>
        /// Name of the Environment variable describing the config file.
        /// </summary>
        /// <returns></returns>
        public string EnvironmentVariable { get; set; }
        #endregion
        #region Public Methods  
        /// <summary>
        /// decode text
        /// </summary>
        /// <param name="cipher">text to decode</param>
        /// <returns>decoded text</returns>
        public static  string Decode(string cipher)
        {
            return (Encryption.Decrypt(cipher, AppIdentifier));
        }
        /// <summary>
        /// encode credentials password
        /// </summary>
        /// <param name="userId">username to encode</param>
        /// <param name="password">password to encode</param>
        /// <returns></returns>
        public static string Encode(string userId, string password)
        {
            return (Encryption.Encrypt(password, AppIdentifier));
        }
        /// <summary>
        /// compute the path to the config file according to the type <see cref="ConfigType"/>
        /// </summary>
        /// <param name="type">specifies how the config file is determined</param>
        /// <param name="configLocation">Location of the config file. used with ConfigType.Specific as folder for config File and with Config.EnvironmentVariable as the name of the environment variable to be used for retrieving the config file with Path</param>
        /// <param name="configFilename">Name of the config file. with ConfigType.Executeable if omitted the name of the executeable with ".config" is used. mandatory with ConfigType.Profile...</param>
        /// <returns>FullPath to the config file</returns>
        public string SetConfigFile(ConfigType type, string configLocation, string configFilename)
        {
            ConfigFilePath = string.Empty;
            try
            {
                switch (type)
                {
                    case ConfigType.Executeable:
                        ConfigFilePath = GetConfigFilefromExecutable(configFilename);
                        break;
                    case ConfigType.RoamingProfile:
                    case ConfigType.LocalProfile:
                    case ConfigType.CommonProfile:
                        if (string.IsNullOrEmpty(configFilename))
                            throw (new InvalidEnumArgumentException(nameof(configFilename)));
                        ConfigFilePath = GetProfileConfigFile(type, configLocation, configFilename);
                        break;
                    case ConfigType.Specific:
                        ConfigFilePath = GetSpecificConfigFile(configLocation, configFilename);
                        break;
                    case ConfigType.EnvironmentVariable:
                        ConfigFilePath = GetConfigFromEnvironmentVariable(configLocation);
                        break;
                    case ConfigType.Dynamic:
                        
                        ConfigFilePath = GetSpecificConfigFile(configLocation, configFilename);
                        if (!string.IsNullOrEmpty(ConfigFilePath))
                        {
                            Log.Trace($"Configfile specified {ConfigFilePath}");
                            break;
                        }
                        ConfigFilePath = GetConfigFromEnvironmentVariable();
                        if (!string.IsNullOrEmpty(ConfigFilePath))
                        {
                            Log.Trace($"Configfile from Environment {ConfigFilePath}");
                            break;
                        }
                        ConfigFilePath = GetConfigFilefromExecutable(configFilename);
                        if (!string.IsNullOrEmpty(ConfigFilePath))
                        {
                            Log.Trace($"Configfile from Executeable {ConfigFilePath}");
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error setting configfilename:{ex}");
                throw;
            }
            finally
            {
                Log.Warn($"configFile to use {ConfigFilePath}");
                //Configuration = LoadConfiguration();
            }
            return (ConfigFilePath);
        }

        #endregion
        #region To life and die in starlight
        public Config() { }

        protected Config(ConfigType type, string configLocation) : this(type, configLocation, null)
        {

        }
        public Config(ConfigType type, string configLocation, string configFilename)
        {
            SetConfigFile(type, configLocation, configFilename);
        }
        #endregion
        #region Private Methods
        /// <summary>
        /// Extract configFilename from path to executable. If fileNAme is specified it is used for the filename otherwise the executables name is used extended with  .config
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private string GetConfigFilefromExecutable(string fileName)
        {
            Assembly rentyAss = Assembly.GetEntryAssembly();
            
            if (rentyAss == null)
                rentyAss = Assembly.GetCallingAssembly();
            if (rentyAss == null)
                return (null);

            Uri executeable = new Uri(rentyAss.GetName().CodeBase);
            string executeableDirectory = Path.GetDirectoryName(executeable.LocalPath);
            string retVal = string.Empty;
            
            if (string.IsNullOrEmpty(fileName))
                retVal = executeable.LocalPath + ".config";
            else
                retVal = Path.Combine(executeableDirectory, fileName);

            return (retVal);
        }

        private string GetSpecificConfigFile(string location, string fileName)
        {
            string retVal = string.Empty;
            if (!string.IsNullOrEmpty(location))
            {
                if (!string.IsNullOrEmpty(fileName))
                    retVal = Path.Combine(location, fileName);
                else
                    retVal = location;
            }
            else
                retVal = fileName;
            return (retVal);
        }
        private string GetConfigFromEnvironmentVariable(string environmentVariable = null)
        {
            if (!string.IsNullOrEmpty(environmentVariable))
                EnvironmentVariable = environmentVariable;
            return (string.IsNullOrEmpty(EnvironmentVariable) ? string.Empty : Environment.ExpandEnvironmentVariables(EnvironmentVariable));
        }
        private string GetProfileConfigFile(ConfigType type, string profilePath, string configFileName)
        {
            Environment.SpecialFolder folderToUse = Environment.SpecialFolder.ApplicationData;
            switch (type)
            {
                case ConfigType.CommonProfile:
                    folderToUse = Environment.SpecialFolder.CommonApplicationData;
                    break;
                case ConfigType.RoamingProfile:
                    folderToUse = Environment.SpecialFolder.ApplicationData;
                    break;
                case ConfigType.LocalProfile:
                    folderToUse = Environment.SpecialFolder.LocalApplicationData;
                    break;
            }
            string configFilePath = Path.Combine(Environment.GetFolderPath(folderToUse), profilePath);
            configFilePath = Path.Combine(configFilePath, configFileName);
            Aurora.IO.Directory.EnsureFileDirectory(configFilePath);
            return (configFilePath);
        }
        #endregion

    }
}
