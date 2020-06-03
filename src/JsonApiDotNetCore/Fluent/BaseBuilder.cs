using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JsonApiDotNetCore.Fluent
{
    public abstract class BaseBuilder<TResource>
    {
        protected readonly ResourceContext _resourceContext;
        protected readonly IJsonApiOptions _options;

        public BaseBuilder(ResourceContext resourceContext, IJsonApiOptions options)
        {
            _resourceContext = resourceContext;
            _options = options;
        }

        protected string FormatPropertyName(PropertyInfo resourceProperty)
        {
            return _options.SerializerContractResolver
                           .NamingStrategy
                           .GetPropertyName(resourceProperty.Name, false);
        }

        protected List<T> CombineAnnotations<T>(T fluentConfiguration, List<T> annotations, IEqualityComparer<T> comparer)
        {
            List<T> fluentConfigurations = new List<T>();
            fluentConfigurations.Add(fluentConfiguration);

            List<T> combined = new List<T>();

            combined = fluentConfigurations.Union(annotations, comparer)
                                           .ToList();
            
            return combined;
        }

        public abstract void Build();
    }
}
