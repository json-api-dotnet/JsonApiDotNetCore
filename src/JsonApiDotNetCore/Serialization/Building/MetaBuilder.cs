using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Serialization.Building
{
    /// <inheritdoc />
    public class MetaBuilder<TResource> : IMetaBuilder<TResource> where TResource : class, IIdentifiable
    {
        private readonly IPaginationContext _paginationContext;
        private readonly IJsonApiOptions _options;
        private readonly IResourceDefinitionAccessor _resourceDefinitionAccessor;
        private readonly IResponseMeta _responseMeta;

        private Dictionary<string, object> _meta = new Dictionary<string, object>();

        public MetaBuilder(IPaginationContext paginationContext, IJsonApiOptions options, IResourceDefinitionAccessor resourceDefinitionAccessor, IResponseMeta responseMeta = null)
        {
            _paginationContext = paginationContext ?? throw new ArgumentNullException(nameof(paginationContext));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _resourceDefinitionAccessor = resourceDefinitionAccessor ?? throw new ArgumentNullException(nameof(resourceDefinitionAccessor));
            _responseMeta = responseMeta;
        }

        /// <inheritdoc />
        public void Add(string key, object value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            _meta[key] = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <inheritdoc />
        public void Add(IReadOnlyDictionary<string,object> values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));

            _meta = values.Keys.Union(_meta.Keys)
                .ToDictionary(key => key, 
                    key => values.ContainsKey(key) ? values[key] : _meta[key]);
        }

        /// <inheritdoc />
        public IDictionary<string, object> GetMeta()
        {
            if (_paginationContext.TotalResourceCount != null)
            {
                var namingStrategy = _options.SerializerContractResolver.NamingStrategy;
                string key = namingStrategy.GetPropertyName("TotalResources", false);

                _meta.Add(key, _paginationContext.TotalResourceCount);
            }

            if (_responseMeta != null)
            {
                Add(_responseMeta.GetMeta());
            }

            var resourceMeta = _resourceDefinitionAccessor.GetMeta(typeof(TResource));
            if (resourceMeta != null)
            {
                Add(resourceMeta);
            }

            return _meta.Any() ? _meta : null;
        }
    }
}
