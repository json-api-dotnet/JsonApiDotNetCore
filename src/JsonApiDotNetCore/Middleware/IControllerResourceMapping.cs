using JsonApiDotNetCore.Configuration;

namespace JsonApiDotNetCore.Middleware;

/// <summary>
/// Registry of which resource type is associated with which controller.
/// </summary>
public interface IControllerResourceMapping
{
    /// <summary>
    /// Gets the associated resource type for the provided controller type.
    /// </summary>
    ResourceType? GetResourceTypeForController(Type? controllerType);

    /// <summary>
    /// Gets the associated controller name for the provided resource type.
    /// </summary>
    string? GetControllerNameForResourceType(ResourceType? resourceType);
}
