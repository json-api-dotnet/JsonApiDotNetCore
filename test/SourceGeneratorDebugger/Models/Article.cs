using JetBrains.Annotations;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace SourceGeneratorDebugger.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(GenerateControllerEndpoints = JsonApiEndpoints.GetCollection | JsonApiEndpoints.GetSingle | JsonApiEndpoints.GetSecondary)]
public sealed class Article : Identifiable<Guid>
{
    [Attr]
    public string? DisplayName { get; set; }
}
