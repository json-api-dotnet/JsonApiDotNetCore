using JetBrains.Annotations;
using JsonApiDotNetCore.Controllers;

namespace JsonApiDotNetCore.Resources.Annotations;

/// <summary>
/// When put on a resource class, overrides the convention-based public resource name and auto-generates an ASP.NET controller.
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class ResourceAttribute : Attribute
{
    /// <summary>
    /// Optional. The publicly exposed name of this resource type.
    /// </summary>
    public string? PublicName { get; set; }

    /// <summary>
    /// The set of endpoints to auto-generate an ASP.NET controller for. Defaults to <see cref="JsonApiEndpoints.All" />. Set to
    /// <see cref="JsonApiEndpoints.None" /> to disable controller generation.
    /// </summary>
    public JsonApiEndpoints GenerateControllerEndpoints { get; set; } = JsonApiEndpoints.All;

    /// <summary>
    /// Optional. The full namespace in which to auto-generate the ASP.NET controller. Defaults to the sibling namespace "Controllers". For example, a
    /// resource class that is declared in namespace "ExampleCompany.ExampleApi.Models" will use "ExampleCompany.ExampleApi.Controllers" by default.
    /// </summary>
    public string? ControllerNamespace { get; set; }
}
