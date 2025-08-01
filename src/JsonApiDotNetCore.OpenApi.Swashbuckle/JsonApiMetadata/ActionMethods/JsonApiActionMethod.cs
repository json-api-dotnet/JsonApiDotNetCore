using JsonApiDotNetCore.Controllers;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata.ActionMethods;

/// <summary>
/// One of the built-in JSON:API action methods on <see cref="BaseJsonApiController{TResource,TId}" />.
/// </summary>
internal sealed class JsonApiActionMethod(JsonApiEndpoints endpoint, Type controllerType)
    : BuiltinJsonApiActionMethod(controllerType)
{
    public JsonApiEndpoints Endpoint { get; } = endpoint;

    public override string ToString()
    {
        return Endpoint.ToString();
    }
}
