using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Aurora.Configs
{
    public class EnvironmentConfigCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// When overridden in a derived class, creates a new <see cref="T:System.Configuration.ConfigurationElement"/>.
        /// </summary>
        /// <returns>
        /// A newly created <see cref="T:System.Configuration.ConfigurationElement"/>.
        /// </returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return (null);
        }

        /// <summary>
        /// Gets the element key for a specified configuration element when overridden in a derived class.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Object"/> that acts as the key for the specified <see cref="T:System.Configuration.ConfigurationElement"/>.
        /// </returns>
        /// <param name="element">The <see cref="T:System.Configuration.ConfigurationElement"/> to return the key for. </param>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((EnvironmentConfigElement)element).Environment;
        }

        public string[] GetEnvironments()
        {
            return ((from EnvironmentConfigElement target in this
                     select (target.Environment)).ToArray());
        }

        public EnvironmentConfigElement GetElementforEnvironment(string environment)
        {


            EnvironmentConfigElement retVal = (from EnvironmentConfigElement Url in this
                                                   where Url.Environment.Equals(environment)
                                                   select (Url)).FirstOrDefault();
            if (retVal == null && environment == "local")
            {
                retVal = (from EnvironmentConfigElement url in this
                          where url.Environment.Equals("QA")
                          select (url)).FirstOrDefault();
            }
            return (retVal);
        }
        public new IEnumerator<EnvironmentConfigElement> GetEnumerator()
        {
            int count = Count;
            for (int i = 0; i < count; i++)
            {
                yield return BaseGet(i) as EnvironmentConfigElement;
            }
        }
    }
}
