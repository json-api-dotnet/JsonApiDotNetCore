using JetBrains.Annotations;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.RestrictedControllers;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.RestrictedControllers", GenerateControllerEndpoints = ControllerEndpoints)]
public sealed class RelationshipChannel : Channel
{
    internal const JsonApiEndpoints ControllerEndpoints = JsonApiEndpoints.GetRelationship | JsonApiEndpoints.PostRelationship |
        JsonApiEndpoints.PatchRelationship | JsonApiEndpoints.DeleteRelationship;
}
