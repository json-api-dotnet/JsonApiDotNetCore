using System.Text.Json;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Compound;

internal static class ResourceObjectExtensions
{
    public static JsonElement GetAttribute(this ResourceObject resource, string name)
    {
        if (resource.Attributes == null || !resource.Attributes.TryGetValue(name, out object? value))
        {
            throw new InvalidOperationException($"Attribute '{name}' not found on resource of type '{resource.Type}'.");
        }

        return value as JsonElement? ?? JsonSerializer.SerializeToElement(value);
    }
}
