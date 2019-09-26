using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Builders
{
    public class MetaBuilder<T> : IMetaBuilder<T> where T : class, IIdentifiable
    {
        private Dictionary<string, object> _meta = new Dictionary<string, object>();
        private readonly IPageQueryService _pageManager;
        private readonly IJsonApiOptions _options;
        private readonly IRequestMeta _requestMeta;
        private readonly IHasMeta _resourceMeta;

        public MetaBuilder(IPageQueryService pageManager,
                           IJsonApiOptions options,
                           IRequestMeta requestMeta = null,
                           ResourceDefinition<T> resourceDefinition = null)
        {
            _pageManager = pageManager;
            _options = options;
            _requestMeta = requestMeta;
            _resourceMeta = resourceDefinition as IHasMeta;
        }

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
        
        public Dictionary<string, object> GetMeta()
        {
            if (_options.IncludeTotalRecordCount && _pageManager.TotalRecords != null)
                _meta.Add("total-records", _pageManager.TotalRecords);

            if (_requestMeta != null)
                Add(_requestMeta.GetMeta());

            if (_resourceMeta != null)
                Add(_resourceMeta.GetMeta());

            if (_meta.Any()) return _meta;
            return null;
        }
    }
}