using JetBrains.Annotations;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreTests.IntegrationTests.EagerLoading;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class BuildingDefinition : JsonApiResourceDefinition<Building, int>
{
    private readonly IJsonApiRequest _request;

    public BuildingDefinition(IResourceGraph resourceGraph, IJsonApiRequest request)
        : base(resourceGraph)
    {
        ArgumentGuard.NotNull(request);

        _request = request;
    }

    public override void OnDeserialize(Building resource)
    {
        if (_request.WriteOperation == WriteOperationKind.CreateResource)
        {
            // Must ensure that an instance exists for this required relationship,
            // so that ASP.NET ModelState validation does not produce a validation error.
            resource.PrimaryDoor = new Door
            {
                Color = "(unspecified)"
            };
        }
    }
}
