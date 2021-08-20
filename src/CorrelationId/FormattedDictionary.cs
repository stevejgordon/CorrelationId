using System.Collections.Generic;
using System.Linq;

namespace CorrelationId
{
    /// <summary>
    /// Override ToString of Dictionary so that when printed using Console logger
    /// correlation id can be printed out instead of just print the type name
    /// </summary>
    public class FormattedDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        /// <summary>
        /// The joined key value pairs of the dictionary. If empty, will simply return {}
        /// </summary>
        public override string ToString()
        {
            var keyValuePairs = this.Select(x => $"{x.Key}:{x.Value}").ToArray();
            return keyValuePairs.Any() ? string.Join(",", keyValuePairs) : "{}";
        }
    }
}
