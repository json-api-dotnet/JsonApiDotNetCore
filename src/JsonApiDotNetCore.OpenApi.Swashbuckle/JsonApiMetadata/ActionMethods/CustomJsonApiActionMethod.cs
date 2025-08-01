using JsonApiDotNetCore.Controllers;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata.ActionMethods;

/// <summary>
/// A custom action method on <see cref="CoreJsonApiController" />.
/// </summary>
internal sealed class CustomJsonApiActionMethod : OpenApiActionMethod
{
    public static CustomJsonApiActionMethod Instance { get; } = new();

    private CustomJsonApiActionMethod()
    {
    }
}
