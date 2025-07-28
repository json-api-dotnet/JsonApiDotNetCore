namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata.ActionMethods;

/// <summary>
/// An action method in a custom controller, unrelated to JSON:API.
/// </summary>
internal sealed class CustomControllerActionMethod : OpenApiActionMethod
{
    public static CustomControllerActionMethod Instance { get; } = new();

    private CustomControllerActionMethod()
    {
    }
}
