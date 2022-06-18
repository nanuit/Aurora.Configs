using System.Configuration;

namespace Aurora.Configs
{
    public class EnvironmentConfigElement :  CustomConfigElement
    {
        [ConfigurationProperty("environment", DefaultValue = "", IsRequired = true)]
        public string Environment
        {
            get { return (string)this["environment"]; }
            set { this["environment"] = value; }
        }
    }
}
