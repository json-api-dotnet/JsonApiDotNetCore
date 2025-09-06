using JsonApiDotNetCore.Controllers;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata.ActionMethods;

/// <summary>
/// A resource-specific JSON:API action method on <see cref="BaseJsonApiController{TResource,TId}" />.
/// </summary>
internal abstract class ResourceActionMethod(Type controllerType)
    : JsonApiActionMethod(controllerType);
