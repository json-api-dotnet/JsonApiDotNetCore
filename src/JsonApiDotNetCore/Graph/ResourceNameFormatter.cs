using Humanizer;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Fluent;
using Newtonsoft.Json.Serialization;
using System;
using System.Reflection;

namespace JsonApiDotNetCore.Graph
{
    internal sealed class ResourceNameFormatter
    {
        private readonly NamingStrategy _namingStrategy;
        private readonly IResourceMappingService _resourceMappingService;

        public ResourceNameFormatter(IJsonApiOptions options, IResourceMappingService resourceMappingService = null)
        {
            _namingStrategy = options.SerializerContractResolver.NamingStrategy;
            _resourceMappingService = resourceMappingService;
        }

        /// <summary>
        /// Gets the publicly visible resource name from the internal type name.
        /// </summary>
        public string FormatResourceName(Type type)
        {
            try
            {
                // Mapping
                IResourceMapping mapping = null;

                if (_resourceMappingService != null && _resourceMappingService.TryGetResourceMapping(type, out mapping)) 
                {                   
                    if (mapping.Resource != null)
                    {
                        return mapping.Resource.ResourceName;
                    }
                }

                // Annotation
                // [Resource("models"] public class Model : Identifiable { /* ... */ }
                if (type.GetCustomAttribute(typeof(ResourceAttribute)) is ResourceAttribute attribute)
                {
                    return attribute.ResourceName;
                }

                // Convention
                return _namingStrategy.GetPropertyName(type.Name.Pluralize(), false);
            }
            catch (InvalidOperationException exception)
            {
                throw new InvalidOperationException($"Cannot define multiple {nameof(ResourceAttribute)}s on type '{type}'.", exception);
            }
        }
    }
}
