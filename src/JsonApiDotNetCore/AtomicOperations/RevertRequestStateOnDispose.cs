using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.AtomicOperations;

/// <summary>
/// Copies the current request state into a backup, which is restored on dispose.
/// </summary>
internal sealed class RevertRequestStateOnDispose : IDisposable
{
    private readonly IJsonApiRequest _sourceRequest;
    private readonly ITargetedFields? _sourceTargetedFields;

    private readonly IJsonApiRequest _backupRequest = new JsonApiRequest();
    private readonly ITargetedFields _backupTargetedFields = new TargetedFields();

    public RevertRequestStateOnDispose(IJsonApiRequest request, ITargetedFields? targetedFields)
    {
        ArgumentGuard.NotNull(request, nameof(request));

        _sourceRequest = request;
        _backupRequest.CopyFrom(request);

        if (targetedFields != null)
        {
            _sourceTargetedFields = targetedFields;
            _backupTargetedFields.CopyFrom(targetedFields);
        }
    }

    public void Dispose()
    {
        _sourceRequest.CopyFrom(_backupRequest);
        _sourceTargetedFields?.CopyFrom(_backupTargetedFields);
    }
}
