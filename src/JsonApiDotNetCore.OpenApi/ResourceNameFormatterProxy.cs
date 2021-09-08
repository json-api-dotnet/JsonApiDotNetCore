using System;
using System.Reflection;
using JsonApiDotNetCore.Resources;
using Newtonsoft.Json.Serialization;

namespace JsonApiDotNetCore.OpenApi
{
    internal sealed class ResourceNameFormatterProxy
    {
        private const string ResourceNameFormatterTypeName = "JsonApiDotNetCore.Configuration.ResourceNameFormatter";
        private const string FormatResourceNameMethodName = "FormatResourceName";

        private readonly NamingStrategy _namingStrategy;
        private readonly Type _resourceNameFormatterType;
        private readonly MethodInfo _formatResourceNameMethod;

        public ResourceNameFormatterProxy(NamingStrategy namingStrategy)
        {
            ArgumentGuard.NotNull(namingStrategy, nameof(namingStrategy));

            _namingStrategy = namingStrategy;

            _resourceNameFormatterType = typeof(IIdentifiable).Assembly.GetType(ResourceNameFormatterTypeName);

            if (_resourceNameFormatterType == null)
            {
                throw new InvalidOperationException($"Failed to locate '{ResourceNameFormatterTypeName}'.");
            }

            _formatResourceNameMethod = _resourceNameFormatterType.GetMethod(FormatResourceNameMethodName);

            if (_formatResourceNameMethod == null)
            {
                throw new InvalidOperationException($"Failed to locate '{ResourceNameFormatterTypeName}.{FormatResourceNameMethodName}'.");
            }
        }

        public string FormatResourceName(Type type)
        {
            ArgumentGuard.NotNull(type, nameof(type));

            object resourceNameFormatter = Activator.CreateInstance(_resourceNameFormatterType, _namingStrategy);

            return (string)_formatResourceNameMethod.Invoke(resourceNameFormatter, new object[]
            {
                type
            });
        }
    }
}
