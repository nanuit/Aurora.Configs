namespace Aurora.Configs
{
    public abstract class EnvironmentConfig : ConfigFromConfigSection
    {
        #region Abstract Members
        /// <summary>
        /// Encryption application identifier
        /// </summary>
        /// <returns>application Identifier for encryption</returns>
        protected abstract string GetEnvironment();
        #endregion             
        protected string m_Environment;

        public string EnvironmentOverride { get; set; }

        /// <summary>
        /// environment to use with the configuration
        /// </summary>
        public string Environment
        {
            get
            {
                m_Environment = EnvironmentOverride;
                if (m_Environment == null)
                    m_Environment = GetEnvironment();
                return (m_Environment);
            }
        }

        public EnvironmentConfig(ConfigType type, string configLocation, string configFilename) : base (type, configLocation, configFilename)
        {
            
        }
    }
}
