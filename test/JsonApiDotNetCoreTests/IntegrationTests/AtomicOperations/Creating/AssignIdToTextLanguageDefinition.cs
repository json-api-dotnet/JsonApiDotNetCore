using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.Creating;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class AssignIdToTextLanguageDefinition(IResourceGraph resourceGraph, ResourceDefinitionHitCounter hitCounter, OperationsDbContext dbContext)
    : ImplicitlyChangingTextLanguageDefinition(resourceGraph, hitCounter, dbContext)
{
    public override Task OnWritingAsync(TextLanguage resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
    {
        if (writeOperation == WriteOperationKind.CreateResource && resource.Id == Guid.Empty)
        {
            resource.Id = Guid.NewGuid();
        }

        return Task.CompletedTask;
    }
}
