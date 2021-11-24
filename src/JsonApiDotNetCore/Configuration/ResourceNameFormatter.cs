using System;
using System.Reflection;
using System.Text.Json;
using Humanizer;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Configuration
{
    internal sealed class ResourceNameFormatter
    {
        private readonly JsonNamingPolicy? _namingPolicy;

        public ResourceNameFormatter(JsonNamingPolicy? namingPolicy)
        {
            _namingPolicy = namingPolicy;
        }

        /// <summary>
        /// Gets the publicly exposed resource name by applying the configured naming convention on the pluralized CLR type name.
        /// </summary>
        public string FormatResourceName(Type resourceClrType)
        {
            ArgumentGuard.NotNull(resourceClrType, nameof(resourceClrType));

            var resourceAttribute = resourceClrType.GetCustomAttribute<ResourceAttribute>(true);

            if (resourceAttribute != null)
            {
                return resourceAttribute.PublicName;
            }

            string publicName = resourceClrType.Name.Pluralize();
            return _namingPolicy != null ? _namingPolicy.ConvertName(publicName) : publicName;
        }
    }
}
