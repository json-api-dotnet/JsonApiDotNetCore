using JetBrains.Annotations;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace SourceGeneratorDebugger.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(GenerateControllerEndpoints = JsonApiEndpoints.Query, ControllerNamespace = "Some.Namespace.To.Place.Controllers")]
public sealed class Account : Identifiable<string>
{
    [Attr]
    public string? DisplayName { get; set; }
}
