using System.Reflection;
using System.Text.Json;
using Humanizer;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Configuration;

internal sealed class ResourceNameFormatter(JsonNamingPolicy? namingPolicy)
{
    private readonly JsonNamingPolicy? _namingPolicy = namingPolicy;

    /// <summary>
    /// Gets the publicly exposed resource name by applying the configured naming convention on the pluralized CLR type name.
    /// </summary>
    public string FormatResourceName(Type resourceClrType)
    {
        ArgumentNullException.ThrowIfNull(resourceClrType);

        var resourceAttribute = resourceClrType.GetCustomAttribute<ResourceAttribute>(true);

        if (resourceAttribute != null && !string.IsNullOrWhiteSpace(resourceAttribute.PublicName))
        {
            return resourceAttribute.PublicName;
        }

        string publicName = resourceClrType.Name.Pluralize();
        return _namingPolicy != null ? _namingPolicy.ConvertName(publicName) : publicName;
    }
}
