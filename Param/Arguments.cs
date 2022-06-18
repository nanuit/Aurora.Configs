using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ServiceStack;

namespace Aurora.Configs.Param
{

    /// <summary>
    /// Arguments class
    /// </summary>
    public class Arguments
    {

        #region Private Members
        private readonly System.Collections.Specialized.StringDictionary m_NamedParameters = new System.Collections.Specialized.StringDictionary();
        private readonly List<string> m_IndexedParameters = new List<string>();
        #endregion
        #region Properties
        /// <summary>
        /// number of named parameters 
        /// </summary>
        public int NameParameterCount => m_NamedParameters.Count;
        /// <summary>
        /// number of indexed parameters 
        /// </summary>
        public int IndexedParameterCount => m_IndexedParameters.Count;

        #endregion
        #region To Life and Die in starlight
        /// <summary>
        /// evaluates the commandline argumentskeine Ahnung and constructs the Named and Indexed parameter list
        /// </summary>
        /// <param name="args">commandline arguments</param>
        public Arguments(IEnumerable<string> args)
        {

            Regex spliter = new Regex(@"^-{1,2}|^/|=",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            Regex remover = new Regex(@"^['""]?(.*?)['""]?$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            string parameter = null;

            // Valid parameters forms:
            // {-,/,--}param{ ,=,:}((",')value(",'))
            // Examples: 
            // -param1 value1 --param2 /param3:"Test-:-work" /param4=happy -param5 '--=nice=--'
            foreach (string argument in args)
            {
                // Look for new parameters (-,/ or --) and a
                // possible enclosed value (=,:)
                var parts = spliter.Split(argument, 3);

                switch (parts.Length)
                {
                    // Found a value (for the last parameter found (space separator))
                    case 1:
                        if (parameter != null)
                        {
                            if (!m_NamedParameters.ContainsKey(parameter))
                            {
                                parts[0] =
                                    remover.Replace(parts[0], "$1");

                                m_NamedParameters.Add(parameter, parts[0]);
                            }
                            parameter = null;
                        }
                        else
                        {
                            m_IndexedParameters.Add(argument);
                        }
                        // else Error: no parameter waiting for a value (skipped)
                        break;

                    // Found just a parameter
                    case 2:
                        // The last parameter is still waiting. 
                        // With no value, set it to true.
                        if (parameter != null)
                        {
                            if (!m_NamedParameters.ContainsKey(parameter))
                                m_NamedParameters.Add(parameter, "true");
                        }
                        parameter = parts[1];
                        break;

                    // Parameter with enclosed value
                    case 3:
                        // The last parameter is still waiting. 
                        // With no value, set it to true.
                        if (parameter != null)
                        {
                            if (!m_NamedParameters.ContainsKey(parameter))
                                m_NamedParameters.Add(parameter, "true");
                        }

                        parameter = parts[1];

                        // Remove possible enclosing characters (",')
                        if (!m_NamedParameters.ContainsKey(parameter))
                        {
                            parts[2] = remover.Replace(parts[2], "$1");
                            m_NamedParameters.Add(parameter, parts[2]);
                        }

                        parameter = null;
                        break;
                }
            }
            // In case a parameter is still waiting
            if (parameter != null)
            {
                if (!m_NamedParameters.ContainsKey(parameter))
                    m_NamedParameters.Add(parameter, "true");
            }
        }

        #endregion
        #region Public Methods
        // Retrieve a parameter value if it exists (overriding C# indexer property)
        public string this[string param] => (m_NamedParameters[param]);
        /// <summary>
        /// retrieve the enumerator for unnamed parameters
        /// </summary>
        /// <returns></returns>
        public IEnumerator<string> GetParameterEnumerator()
        {
            return (m_IndexedParameters.GetEnumerator());
        }

        /// <summary>
        /// get the numbered parameter and convert it to the given type
        /// </summary>
        /// <typeparam name="T">type to be returned</typeparam>
        /// <param name="paramNumber">index of the parameter</param>
        /// <returns></returns>
        public T GetParameter<T>(int paramNumber)
        {
            T retVal = default(T);
            if (m_IndexedParameters.Count < paramNumber) return (retVal);
            try
            {
                retVal = (T)(object)(m_IndexedParameters[paramNumber - 1]);
            }
            catch { }

            return (retVal);
        }
        /// <summary>
        /// check if a parameter with the given name exists
        /// </summary>
        /// <param name="paramName">name of the parameter to check for</param>
        /// <returns>indicates if a parameter with the given name exists</returns>
        public bool HasParameter(string paramName)
        {
            return (m_NamedParameters.ContainsKey(paramName) || m_IndexedParameters.Contains(paramName));
        }
        /// <summary>
        /// get the named parameter and convert it to the given type
        /// </summary>
        /// <typeparam name="T">type to be returned</typeparam>
        /// <param name="param">name of the parameter</param>
        /// <returns></returns>
        public T GetParameter<T>(string param)
        {
            T retVal = default(T);
            if (!m_NamedParameters.ContainsKey(param)) return (retVal);
            try
            {
                retVal = (T)Convert.ChangeType(m_NamedParameters[param], typeof(T));
            }
            catch { }
            return (retVal);
        }
        #endregion
    }
}

