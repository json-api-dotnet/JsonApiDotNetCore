using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources.Annotations;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Resources
{
    /// <inheritdoc />
    [PublicAPI]
    public sealed class ResourceChangeTracker<TResource> : IResourceChangeTracker<TResource>
        where TResource : class, IIdentifiable
    {
        private readonly IJsonApiOptions _options;
        private readonly IResourceContextProvider _resourceContextProvider;
        private readonly ITargetedFields _targetedFields;

        private IDictionary<string, string> _initiallyStoredAttributeValues;
        private IDictionary<string, string> _requestedAttributeValues;
        private IDictionary<string, string> _finallyStoredAttributeValues;

        public ResourceChangeTracker(IJsonApiOptions options, IResourceContextProvider resourceContextProvider, ITargetedFields targetedFields)
        {
            ArgumentGuard.NotNull(options, nameof(options));
            ArgumentGuard.NotNull(resourceContextProvider, nameof(resourceContextProvider));
            ArgumentGuard.NotNull(targetedFields, nameof(targetedFields));

            _options = options;
            _resourceContextProvider = resourceContextProvider;
            _targetedFields = targetedFields;
        }

        /// <inheritdoc />
        public void SetInitiallyStoredAttributeValues(TResource resource)
        {
            ArgumentGuard.NotNull(resource, nameof(resource));

            ResourceContext resourceContext = _resourceContextProvider.GetResourceContext<TResource>();
            _initiallyStoredAttributeValues = CreateAttributeDictionary(resource, resourceContext.Attributes);
        }

        /// <inheritdoc />
        public void SetRequestedAttributeValues(TResource resource)
        {
            ArgumentGuard.NotNull(resource, nameof(resource));

            _requestedAttributeValues = CreateAttributeDictionary(resource, _targetedFields.Attributes);
        }

        /// <inheritdoc />
        public void SetFinallyStoredAttributeValues(TResource resource)
        {
            ArgumentGuard.NotNull(resource, nameof(resource));

            ResourceContext resourceContext = _resourceContextProvider.GetResourceContext<TResource>();
            _finallyStoredAttributeValues = CreateAttributeDictionary(resource, resourceContext.Attributes);
        }

        private IDictionary<string, string> CreateAttributeDictionary(TResource resource, IEnumerable<AttrAttribute> attributes)
        {
            var result = new Dictionary<string, string>();

            foreach (AttrAttribute attribute in attributes)
            {
                object value = attribute.GetValue(resource);
                string json = JsonConvert.SerializeObject(value, _options.SerializerSettings);
                result.Add(attribute.PublicName, json);
            }

            return result;
        }

        /// <inheritdoc />
        public bool HasImplicitChanges()
        {
            foreach (string key in _initiallyStoredAttributeValues.Keys)
            {
                if (_requestedAttributeValues.ContainsKey(key))
                {
                    string requestedValue = _requestedAttributeValues[key];
                    string actualValue = _finallyStoredAttributeValues[key];

                    if (requestedValue != actualValue)
                    {
                        return true;
                    }
                }
                else
                {
                    string initiallyStoredValue = _initiallyStoredAttributeValues[key];
                    string finallyStoredValue = _finallyStoredAttributeValues[key];

                    if (initiallyStoredValue != finallyStoredValue)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
