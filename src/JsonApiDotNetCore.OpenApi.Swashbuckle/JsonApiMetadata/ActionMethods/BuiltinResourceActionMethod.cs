using JsonApiDotNetCore.Controllers;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata.ActionMethods;

/// <summary>
/// One of the built-in JSON:API action methods on <see cref="BaseJsonApiController{TResource,TId}" />.
/// </summary>
internal sealed class BuiltinResourceActionMethod(JsonApiEndpoints endpoint, Type controllerType)
    : ResourceActionMethod(controllerType)
{
    public JsonApiEndpoints Endpoint { get; } = endpoint;

    public override string ToString()
    {
        return Endpoint.ToString();
    }
}
