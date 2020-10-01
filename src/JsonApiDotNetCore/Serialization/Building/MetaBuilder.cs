using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;

namespace JsonApiDotNetCore.Serialization.Building
{
    /// <inheritdoc />
    public class MetaBuilder : IMetaBuilder
    {
        private readonly IPaginationContext _paginationContext;
        private readonly IJsonApiOptions _options;
        private readonly IResponseMeta _responseMeta;

        private Dictionary<string, object> _meta = new Dictionary<string, object>();

        public MetaBuilder(IPaginationContext paginationContext, IJsonApiOptions options, IResponseMeta responseMeta)
        {
            _paginationContext = paginationContext ?? throw new ArgumentNullException(nameof(paginationContext));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _responseMeta = responseMeta ?? throw new ArgumentNullException(nameof(responseMeta));
        }

        /// <inheritdoc />
        public void Add(IReadOnlyDictionary<string, object> values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));

            _meta = values.Keys.Union(_meta.Keys)
                .ToDictionary(key => key,
                    key => values.ContainsKey(key) ? values[key] : _meta[key]);
        }

        /// <inheritdoc />
        public IDictionary<string, object> Build()
        {
            if (_paginationContext.TotalResourceCount != null)
            {
                var namingStrategy = _options.SerializerContractResolver.NamingStrategy;
                string key = namingStrategy.GetPropertyName("TotalResources", false);

                _meta.Add(key, _paginationContext.TotalResourceCount);
            }

            var extraMeta = _responseMeta.GetMeta();
            if (extraMeta != null)
            {
                Add(extraMeta);
            }

            return _meta.Any() ? _meta : null;
        }
    }
}
