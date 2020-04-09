using System;
using System.Reflection;
using Humanizer;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Models;
using Newtonsoft.Json.Serialization;

namespace JsonApiDotNetCore.Graph
{
    internal sealed class ResourceNameFormatter
    {
        private readonly NamingStrategy _namingStrategy;

        public ResourceNameFormatter(IJsonApiOptions options)
        {
            var contractResolver = (DefaultContractResolver) options.SerializerSettings.ContractResolver;
            _namingStrategy = contractResolver.NamingStrategy;
        }

        /// <summary>
        /// Gets the publicly visible resource name from the internal type name.
        /// </summary>
        public string FormatResourceName(Type type)
        {
            try
            {
                // check the class definition first
                // [Resource("models"] public class Model : Identifiable { /* ... */ }
                if (type.GetCustomAttribute(typeof(ResourceAttribute)) is ResourceAttribute attribute)
                {
                    return attribute.ResourceName;
                }

                return _namingStrategy.GetPropertyName(type.Name.Pluralize(), false);
            }
            catch (InvalidOperationException exception)
            {
                throw new InvalidOperationException($"Cannot define multiple {nameof(ResourceAttribute)}s on type '{type}'.", exception);
            }
        }
    }
}
