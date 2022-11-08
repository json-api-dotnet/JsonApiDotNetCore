using JetBrains.Annotations;
using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Serialization.Request.Adapters;

/// <summary>
/// Tracks state while adapting objects from <see cref="JsonApiDotNetCore.Serialization.Objects" /> into the shape that controller actions accept.
/// </summary>
[PublicAPI]
public sealed class RequestAdapterState : IDisposable
{
    private readonly IDisposable? _backupRequestState;

    public IJsonApiRequest InjectableRequest { get; }
    public ITargetedFields InjectableTargetedFields { get; }

    public JsonApiRequest? WritableRequest { get; set; }
    public TargetedFields? WritableTargetedFields { get; set; }

    public RequestAdapterPosition Position { get; } = new();
    public IJsonApiRequest Request => WritableRequest ?? InjectableRequest;

    public RequestAdapterState(IJsonApiRequest request, ITargetedFields targetedFields)
    {
        ArgumentGuard.NotNull(request);
        ArgumentGuard.NotNull(targetedFields);

        InjectableRequest = request;
        InjectableTargetedFields = targetedFields;

        if (request.Kind == EndpointKind.AtomicOperations)
        {
            _backupRequestState = new RevertRequestStateOnDispose(request, targetedFields);
        }
    }

    public void RefreshInjectables()
    {
        if (WritableRequest != null)
        {
            InjectableRequest.CopyFrom(WritableRequest);
        }

        if (WritableTargetedFields != null)
        {
            InjectableTargetedFields.CopyFrom(WritableTargetedFields);
        }
    }

    public void Dispose()
    {
        // For resource requests, we'd like the injected state to become the final state.
        // But for operations, it makes more sense to reset than to reflect the last operation.

        if (_backupRequestState != null)
        {
            _backupRequestState.Dispose();
        }
        else
        {
            RefreshInjectables();
        }
    }
}
