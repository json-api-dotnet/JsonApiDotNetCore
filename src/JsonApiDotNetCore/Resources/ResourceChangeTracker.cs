using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources.Annotations;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Resources
{
    /// <inheritdoc />
    public sealed class ResourceChangeTracker<TResource> : IResourceChangeTracker<TResource> where TResource : class, IIdentifiable
    {
        private readonly IJsonApiOptions _options;
        private readonly IResourceContextProvider _resourceContextProvider;
        private readonly ITargetedFields _targetedFields;

        private IDictionary<string, string> _initiallyStoredAttributeValues;
        private IDictionary<string, string> _requestedAttributeValues;
        private IDictionary<string, string> _finallyStoredAttributeValues;

        public ResourceChangeTracker(IJsonApiOptions options, IResourceContextProvider resourceContextProvider,
            ITargetedFields targetedFields)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _resourceContextProvider = resourceContextProvider ?? throw new ArgumentNullException(nameof(resourceContextProvider));
            _targetedFields = targetedFields ?? throw new ArgumentNullException(nameof(targetedFields));
        }

        /// <inheritdoc />
        public void SetInitiallyStoredAttributeValues(TResource resource)
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));

            var resourceContext = _resourceContextProvider.GetResourceContext<TResource>();
            _initiallyStoredAttributeValues = CreateAttributeDictionary(resource, resourceContext.Attributes);
        }

        /// <inheritdoc />
        public void SetRequestedAttributeValues(TResource resource)
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));

            _requestedAttributeValues = CreateAttributeDictionary(resource, _targetedFields.Attributes);
        }

        /// <inheritdoc />
        public void SetFinallyStoredAttributeValues(TResource resource)
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));

            var resourceContext = _resourceContextProvider.GetResourceContext<TResource>();
            _finallyStoredAttributeValues = CreateAttributeDictionary(resource, resourceContext.Attributes);
        }

        private IDictionary<string, string> CreateAttributeDictionary(TResource resource,
            IEnumerable<AttrAttribute> attributes)
        {
            var result = new Dictionary<string, string>();

            foreach (var attribute in attributes)
            {
                object value = attribute.GetValue(resource);
                var json = JsonConvert.SerializeObject(value, _options.SerializerSettings);
                result.Add(attribute.PublicName, json);
            }

            return result;
        }

        /// <inheritdoc />
        public bool HasImplicitChanges()
        {
            foreach (var key in _initiallyStoredAttributeValues.Keys)
            {
                if (_requestedAttributeValues.ContainsKey(key))
                {
                    var requestedValue = _requestedAttributeValues[key];
                    var actualValue = _finallyStoredAttributeValues[key];

                    if (requestedValue != actualValue)
                    {
                        return true;
                    }
                }
                else
                {
                    var initiallyStoredValue = _initiallyStoredAttributeValues[key];
                    var finallyStoredValue = _finallyStoredAttributeValues[key];

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
