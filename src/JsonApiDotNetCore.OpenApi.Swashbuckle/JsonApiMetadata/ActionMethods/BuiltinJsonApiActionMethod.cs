using JsonApiDotNetCore.Controllers;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata.ActionMethods;

/// <summary>
/// A built-in JSON:API action method on <see cref="CoreJsonApiController" />.
/// </summary>
internal abstract class BuiltinJsonApiActionMethod : OpenApiActionMethod
{
    public Type ControllerType { get; }

    protected BuiltinJsonApiActionMethod(Type controllerType)
    {
        ArgumentNullException.ThrowIfNull(controllerType);

        ControllerType = controllerType;
    }
}
