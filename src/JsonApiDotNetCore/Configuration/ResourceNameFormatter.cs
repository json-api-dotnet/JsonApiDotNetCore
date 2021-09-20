using System;
using System.Reflection;
using System.Text.Json;
using Humanizer;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Configuration
{
    internal sealed class ResourceNameFormatter
    {
        private readonly JsonNamingPolicy _namingPolicy;

        public ResourceNameFormatter(JsonNamingPolicy namingPolicy)
        {
            _namingPolicy = namingPolicy;
        }

        /// <summary>
        /// Gets the publicly visible resource name for the internal type name using the configured naming convention.
        /// </summary>
        public string FormatResourceName(Type resourceType)
        {
            if (resourceType.GetCustomAttribute(typeof(ResourceAttribute)) is ResourceAttribute attribute)
            {
                return attribute.PublicName;
            }

            string publicName = resourceType.Name.Pluralize();
            return _namingPolicy != null ? _namingPolicy.ConvertName(publicName) : publicName;
        }
    }
}
