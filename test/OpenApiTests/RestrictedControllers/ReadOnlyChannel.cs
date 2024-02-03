using JetBrains.Annotations;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.RestrictedControllers;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.RestrictedControllers", GenerateControllerEndpoints = ControllerEndpoints)]
public sealed class ReadOnlyChannel : Channel
{
    internal const JsonApiEndpoints ControllerEndpoints = JsonApiEndpoints.Query;
}
