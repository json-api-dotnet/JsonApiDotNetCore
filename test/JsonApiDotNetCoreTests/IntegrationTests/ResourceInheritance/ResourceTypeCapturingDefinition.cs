using JetBrains.Annotations;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance;

/// <summary>
/// Enables tests to verify which resource types are being observed.
/// </summary>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class ResourceTypeCapturingDefinition<TResource, TId> : JsonApiResourceDefinition<TResource, TId>
    where TResource : class, IIdentifiable<TId>
{
    private readonly IJsonApiRequest _request;
    private readonly ResourceTypeCaptureStore<TResource, TId> _captureStore;

    public ResourceTypeCapturingDefinition(IResourceGraph resourceGraph, IJsonApiRequest request, ResourceTypeCaptureStore<TResource, TId> captureStore)
        : base(resourceGraph)
    {
        ArgumentGuard.NotNull(request, nameof(request));
        ArgumentGuard.NotNull(captureStore, nameof(captureStore));

        _request = request;
        _captureStore = captureStore;
    }

    public override Task OnPrepareWriteAsync(TResource resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
    {
        EnsureSnapshot(resource);

        return Task.CompletedTask;
    }

    public override Task<IIdentifiable?> OnSetToOneRelationshipAsync(TResource leftResource, HasOneAttribute hasOneRelationship, IIdentifiable? rightResourceId,
        WriteOperationKind writeOperation, CancellationToken cancellationToken)
    {
        EnsureSnapshot(leftResource, rightResourceId);

        return Task.FromResult(rightResourceId);
    }

    public override Task OnSetToManyRelationshipAsync(TResource leftResource, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
        WriteOperationKind writeOperation, CancellationToken cancellationToken)
    {
        EnsureSnapshot(leftResource, rightResourceIds);

        return Task.CompletedTask;
    }

    public override Task OnAddToRelationshipAsync(TResource leftResource, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
        CancellationToken cancellationToken)
    {
        EnsureSnapshot(leftResource, rightResourceIds);

        return Task.CompletedTask;
    }

    public override Task OnRemoveFromRelationshipAsync(TResource leftResource, HasManyAttribute hasManyRelationship, ISet<IIdentifiable> rightResourceIds,
        CancellationToken cancellationToken)
    {
        EnsureSnapshot(leftResource, rightResourceIds);

        return Task.CompletedTask;
    }

    public override Task OnWritingAsync(TResource resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
    {
        EnsureSnapshot(resource);

        return Task.CompletedTask;
    }

    public override Task OnWriteSucceededAsync(TResource resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
    {
        EnsureSnapshot(resource);

        return Task.CompletedTask;
    }

    private void EnsureSnapshot(TResource leftType, IIdentifiable? rightResourceId = null)
    {
        IIdentifiable[] rightResourceIds = rightResourceId != null ? ArrayFactory.Create(rightResourceId) : Array.Empty<IIdentifiable>();

        EnsureSnapshot(leftType, rightResourceIds);
    }

    private void EnsureSnapshot(TResource leftType, IEnumerable<IIdentifiable> rightResourceIds)
    {
        if (_captureStore.Request == null)
        {
            _captureStore.Request = TakeRequestSnapshot();
            _captureStore.LeftDeclaredType = typeof(TResource);
            _captureStore.LeftReflectedTypeName = leftType.GetType().Name;
        }
        else
        {
            if (leftType.GetType().Name != _captureStore.LeftReflectedTypeName)
            {
                throw new InvalidOperationException(
                    $"Reflected left type changed from '{_captureStore.LeftReflectedTypeName}' to '{leftType.GetType().Name}'.");
            }
        }

        foreach (IIdentifiable rightResourceId in rightResourceIds)
        {
            _captureStore.RightTypeNames.Add(rightResourceId.GetType().Name);
        }
    }

    private IJsonApiRequest TakeRequestSnapshot()
    {
        var requestSnapshot = new JsonApiRequest();
        requestSnapshot.CopyFrom(_request);

        return requestSnapshot;
    }
}
