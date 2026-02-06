using JetBrains.Annotations;
using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;

namespace JsonApiDotNetCore.Middleware;

/// <summary>
/// Enables removing JSON:API controller action methods at startup. For atomic:operation requests, see <see cref="IAtomicOperationFilter" />.
/// </summary>
[PublicAPI]
public interface IJsonApiEndpointFilter
{
    /// <summary>
    /// Determines whether to remove the associated controller action method.
    /// </summary>
    /// <param name="resourceType">
    /// The primary resource type of the endpoint.
    /// </param>
    /// <param name="endpoint">
    /// The JSON:API endpoint. Despite <see cref="JsonApiEndpoints" /> being a <see cref="FlagsAttribute" /> enum, a single value is always passed here.
    /// </param>
    bool IsEnabled(ResourceType resourceType, JsonApiEndpoints endpoint);
}
