using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreTests.IntegrationTests.ReadWrite.Creating;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
internal sealed class AssignIdToRgbColorDefinition(IResourceGraph resourceGraph) : JsonApiResourceDefinition<RgbColor, string?>(resourceGraph)
{
    internal const string DefaultId = "0x000000";
    internal const string DefaultName = "Black";

    public override Task OnWritingAsync(RgbColor resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
    {
        if (writeOperation == WriteOperationKind.CreateResource && resource.Id == null)
        {
            SetDefaultColor(resource);
        }

        return Task.CompletedTask;
    }

    private static void SetDefaultColor(RgbColor resource)
    {
        resource.Id = DefaultId;
        resource.DisplayName = DefaultName;
    }
}
