using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;

namespace JsonApiDotNetCore.AtomicOperations;

/// <inheritdoc />
public sealed class LocalIdTracker : ILocalIdTracker
{
    private readonly IDictionary<string, LocalIdState> _idsTracked = new Dictionary<string, LocalIdState>();

    /// <inheritdoc />
    public void Reset()
    {
        _idsTracked.Clear();
    }

    /// <inheritdoc />
    public void Declare(string localId, ResourceType resourceType)
    {
        ArgumentGuard.NotNullNorEmpty(localId, nameof(localId));
        ArgumentGuard.NotNull(resourceType, nameof(resourceType));

        AssertIsNotDeclared(localId);

        _idsTracked[localId] = new LocalIdState(resourceType);
    }

    private void AssertIsNotDeclared(string localId)
    {
        if (_idsTracked.ContainsKey(localId))
        {
            throw new DuplicateLocalIdValueException(localId);
        }
    }

    /// <inheritdoc />
    public void Assign(string localId, ResourceType resourceType, string stringId)
    {
        ArgumentGuard.NotNullNorEmpty(localId, nameof(localId));
        ArgumentGuard.NotNull(resourceType, nameof(resourceType));
        ArgumentGuard.NotNullNorEmpty(stringId, nameof(stringId));

        AssertIsDeclared(localId);

        LocalIdState item = _idsTracked[localId];

        AssertSameResourceType(resourceType, item.ResourceType, localId);

        if (item.ServerId != null)
        {
            throw new InvalidOperationException($"Cannot reassign to existing local ID '{localId}'.");
        }

        item.ServerId = stringId;
    }

    /// <inheritdoc />
    public string GetValue(string localId, ResourceType resourceType)
    {
        ArgumentGuard.NotNullNorEmpty(localId, nameof(localId));
        ArgumentGuard.NotNull(resourceType, nameof(resourceType));

        AssertIsDeclared(localId);

        LocalIdState item = _idsTracked[localId];

        AssertSameResourceType(resourceType, item.ResourceType, localId);

        if (item.ServerId == null)
        {
            throw new LocalIdSingleOperationException(localId);
        }

        return item.ServerId;
    }

    private void AssertIsDeclared(string localId)
    {
        if (!_idsTracked.ContainsKey(localId))
        {
            throw new UnknownLocalIdValueException(localId);
        }
    }

    private static void AssertSameResourceType(ResourceType currentType, ResourceType declaredType, string localId)
    {
        if (!declaredType.Equals(currentType))
        {
            throw new IncompatibleLocalIdTypeException(localId, declaredType.PublicName, currentType.PublicName);
        }
    }

    private sealed class LocalIdState
    {
        public ResourceType ResourceType { get; }
        public string? ServerId { get; set; }

        public LocalIdState(ResourceType resourceType)
        {
            ResourceType = resourceType;
        }
    }
}
