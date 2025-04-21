using JetBrains.Annotations;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.OpenApiGenerationFailures.MissingFromBody;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.OpenApiGenerationFailures.MissingFromBody", GenerateControllerEndpoints = JsonApiEndpoints.None)]
public sealed class RecycleBin : Identifiable<long>
{
    [Attr]
    public bool IsEmpty { get; set; }
}
