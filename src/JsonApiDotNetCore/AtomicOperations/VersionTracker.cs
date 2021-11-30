using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.AtomicOperations;

public sealed class VersionTracker : IVersionTracker
{
    private static readonly CollectionConverter CollectionConverter = new();

    private readonly ITargetedFields _targetedFields;
    private readonly IJsonApiRequest _request;
    private readonly Dictionary<string, string> _versionPerResource = new();

    public VersionTracker(ITargetedFields targetedFields, IJsonApiRequest request)
    {
        ArgumentGuard.NotNull(targetedFields, nameof(targetedFields));
        ArgumentGuard.NotNull(request, nameof(request));

        _targetedFields = targetedFields;
        _request = request;
    }

    public bool RequiresVersionTracking()
    {
        if (_request.Kind != EndpointKind.AtomicOperations)
        {
            return false;
        }

        return _request.PrimaryResourceType!.IsVersioned || _targetedFields.Relationships.Any(relationship => relationship.RightType.IsVersioned);
    }

    public void CaptureVersions(ResourceType resourceType, IIdentifiable resource)
    {
        if (_request.Kind == EndpointKind.AtomicOperations)
        {
            if (resourceType.IsVersioned)
            {
                string? leftVersion = resource.GetVersion();
                SetVersion(resourceType, resource.StringId!, leftVersion);
            }

            foreach (RelationshipAttribute relationship in _targetedFields.Relationships)
            {
                if (relationship.RightType.IsVersioned)
                {
                    CaptureVersionsInRelationship(resource, relationship);
                }
            }
        }
    }

    private void CaptureVersionsInRelationship(IIdentifiable resource, RelationshipAttribute relationship)
    {
        object? afterRightValue = relationship.GetValue(resource);
        IReadOnlyCollection<IIdentifiable> afterRightResources = CollectionConverter.ExtractResources(afterRightValue);

        foreach (IIdentifiable rightResource in afterRightResources)
        {
            string? rightVersion = rightResource.GetVersion();
            SetVersion(relationship.RightType, rightResource.StringId!, rightVersion);
        }
    }

    private void SetVersion(ResourceType resourceType, string stringId, string? version)
    {
        string key = GetKey(resourceType, stringId);

        if (version == null)
        {
            _versionPerResource.Remove(key);
        }
        else
        {
            _versionPerResource[key] = version;
        }
    }

    public string? GetVersion(ResourceType resourceType, string stringId)
    {
        string key = GetKey(resourceType, stringId);
        return _versionPerResource.TryGetValue(key, out string? version) ? version : null;
    }

    private string GetKey(ResourceType resourceType, string stringId)
    {
        return $"{resourceType.PublicName}::{stringId}";
    }
}
