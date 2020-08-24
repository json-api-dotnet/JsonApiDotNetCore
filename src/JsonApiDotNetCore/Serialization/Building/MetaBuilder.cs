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
        private Dictionary<string, object> _meta = new Dictionary<string, object>();
        private readonly IPaginationContext _paginationContext;
        private readonly IJsonApiOptions _options;
        private readonly IRequestMeta _requestMeta;
        private readonly IHasMeta _resourceMeta;

        public MetaBuilder(IPaginationContext paginationContext, IJsonApiOptions options, IRequestMeta requestMeta = null,
            ResourceDefinition<TResource> resourceDefinition = null)
        {
            _paginationContext = paginationContext ?? throw new ArgumentNullException(nameof(paginationContext));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _requestMeta = requestMeta;
            _resourceMeta = resourceDefinition as IHasMeta;
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

            if (_requestMeta != null)
            {
                Add(_requestMeta.GetMeta());
            }

            if (_resourceMeta != null)
            {
                Add(_resourceMeta.GetMeta());
            }

            return _meta.Any() ? _meta : null;
        }
    }
}
