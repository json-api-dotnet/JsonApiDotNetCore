using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;

namespace JsonApiDotNetCore.AtomicOperations;

/// <inheritdoc cref="ILocalIdTracker" />
public sealed class LocalIdTracker : ILocalIdTracker
{
    private readonly Dictionary<string, LocalIdState> _idsTracked = new();

    /// <inheritdoc />
    public void Reset()
    {
        _idsTracked.Clear();
    }

    /// <inheritdoc />
    public void Declare(string localId, ResourceType resourceType)
    {
        ArgumentException.ThrowIfNullOrEmpty(localId);
        ArgumentNullException.ThrowIfNull(resourceType);

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
        ArgumentException.ThrowIfNullOrEmpty(localId);
        ArgumentNullException.ThrowIfNull(resourceType);
        ArgumentException.ThrowIfNullOrEmpty(stringId);

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
        ArgumentException.ThrowIfNullOrEmpty(localId);
        ArgumentNullException.ThrowIfNull(resourceType);

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

    private sealed class LocalIdState(ResourceType resourceType)
    {
        public ResourceType ResourceType { get; } = resourceType;
        public string? ServerId { get; set; }
    }
}
