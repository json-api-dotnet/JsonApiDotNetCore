using System.Collections.Generic;
using System.Linq;

namespace JsonApiDotNetCore.Builders
{
    public class MetaBuilder : IMetaBuilder
    {
        private Dictionary<string, object> _meta = new Dictionary<string, object>();

        public void Add(string key, object value)
        {
            _meta[key] = value;
        }

        /// <summary>
        /// Joins the new dictionary with the current one. In the event of a key collision,
        /// the new value will override the old.
        /// </summary>
        public void Add(Dictionary<string,object> values)
        {
            _meta = values.Keys.Union(_meta.Keys)
                .ToDictionary(key => key, 
                    key => values.ContainsKey(key) ? values[key] : _meta[key]);
        }
        
        public Dictionary<string, object> Build()
        {
            return _meta;
        }
    }
}