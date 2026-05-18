using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;

namespace JsonApiDotNetCore.Middleware;

internal sealed class AlwaysEnabledJsonApiEndpointFilter : IJsonApiEndpointFilter
{
    /// <inheritdoc />
    public bool IsEnabled(ResourceType resourceType, JsonApiEndpoints endpoint)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        return true;
    }
}
