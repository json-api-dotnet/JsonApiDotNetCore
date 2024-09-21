using DapperExample.TranslationToSql.DataModel;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace DapperExample.Repositories;

/// <summary>
/// A simplistic change detector. Detects changes in scalar properties, but relationship changes only one level deep.
/// </summary>
internal sealed class ResourceChangeDetector
{
    private readonly CollectionConverter _collectionConverter = new();
    private readonly IDataModelService _dataModelService;

    private Dictionary<string, object?> _currentColumnValues = [];
    private Dictionary<string, object?> _newColumnValues = [];

    private Dictionary<RelationshipAttribute, HashSet<IIdentifiable>> _currentRightResourcesByRelationship = [];
    private Dictionary<RelationshipAttribute, HashSet<IIdentifiable>> _newRightResourcesByRelationship = [];

    public ResourceType ResourceType { get; }

    public ResourceChangeDetector(ResourceType resourceType, IDataModelService dataModelService)
    {
        ArgumentGuard.NotNull(resourceType);
        ArgumentGuard.NotNull(dataModelService);

        ResourceType = resourceType;
        _dataModelService = dataModelService;
    }

    public void CaptureCurrentValues(IIdentifiable resource)
    {
        ArgumentGuard.NotNull(resource);
        AssertSameType(ResourceType, resource);

        _currentColumnValues = CaptureColumnValues(resource);
        _currentRightResourcesByRelationship = CaptureRightResourcesByRelationship(resource);
    }

    public void CaptureNewValues(IIdentifiable resource)
    {
        ArgumentGuard.NotNull(resource);
        AssertSameType(ResourceType, resource);

        _newColumnValues = CaptureColumnValues(resource);
        _newRightResourcesByRelationship = CaptureRightResourcesByRelationship(resource);
    }

    private Dictionary<string, object?> CaptureColumnValues(IIdentifiable resource)
    {
        Dictionary<string, object?> columnValues = [];

        foreach ((string columnName, ResourceFieldAttribute? _) in _dataModelService.GetColumnMappings(ResourceType))
        {
            columnValues[columnName] = _dataModelService.GetColumnValue(ResourceType, resource, columnName);
        }

        return columnValues;
    }

    private Dictionary<RelationshipAttribute, HashSet<IIdentifiable>> CaptureRightResourcesByRelationship(IIdentifiable resource)
    {
        Dictionary<RelationshipAttribute, HashSet<IIdentifiable>> relationshipValues = [];

        foreach (RelationshipAttribute relationship in ResourceType.Relationships)
        {
            object? rightValue = relationship.GetValue(resource);
            HashSet<IIdentifiable> rightResources = _collectionConverter.ExtractResources(rightValue).ToHashSet(IdentifiableComparer.Instance);

            relationshipValues[relationship] = rightResources;
        }

        return relationshipValues;
    }

    public void AssertIsNotClearingAnyRequiredToOneRelationships(string resourceName)
    {
        foreach ((RelationshipAttribute relationship, ISet<IIdentifiable> newRightResources) in _newRightResourcesByRelationship)
        {
            if (relationship is HasOneAttribute hasOneRelationship)
            {
                RelationshipForeignKey foreignKey = _dataModelService.GetForeignKey(hasOneRelationship);

                if (!foreignKey.IsNullable)
                {
                    object? currentRightId =
                        _currentRightResourcesByRelationship.TryGetValue(hasOneRelationship, out HashSet<IIdentifiable>? currentRightResources)
                            ? currentRightResources.FirstOrDefault()?.GetTypedId()
                            : null;

                    object? newRightId = newRightResources.SingleOrDefault()?.GetTypedId();

                    bool hasChanged = !Equals(currentRightId, newRightId);

                    if (hasChanged && newRightId == null)
                    {
                        throw new CannotClearRequiredRelationshipException(relationship.PublicName, resourceName);
                    }
                }
            }
        }
    }

    public IReadOnlyDictionary<HasOneAttribute, (object? currentRightId, object newRightId)> GetOneToOneRelationshipsChangedToNotNull()
    {
        Dictionary<HasOneAttribute, (object? currentRightId, object newRightId)> changes = [];

        foreach ((RelationshipAttribute relationship, ISet<IIdentifiable> newRightResources) in _newRightResourcesByRelationship)
        {
            if (relationship is HasOneAttribute { IsOneToOne: true } hasOneRelationship)
            {
                object? newRightId = newRightResources.SingleOrDefault()?.GetTypedId();

                if (newRightId != null)
                {
                    object? currentRightId =
                        _currentRightResourcesByRelationship.TryGetValue(hasOneRelationship, out HashSet<IIdentifiable>? currentRightResources)
                            ? currentRightResources.FirstOrDefault()?.GetTypedId()
                            : null;

                    if (!Equals(currentRightId, newRightId))
                    {
                        changes[hasOneRelationship] = (currentRightId, newRightId);
                    }
                }
            }
        }

        return changes.AsReadOnly();
    }

    public IReadOnlyDictionary<string, object?> GetChangedColumnValues()
    {
        Dictionary<string, object?> changes = [];

        foreach ((string columnName, object? newColumnValue) in _newColumnValues)
        {
            bool currentFound = _currentColumnValues.TryGetValue(columnName, out object? currentColumnValue);

            if (!currentFound || !Equals(currentColumnValue, newColumnValue))
            {
                changes[columnName] = newColumnValue;
            }
        }

        return changes.AsReadOnly();
    }

    public IReadOnlyDictionary<HasOneAttribute, (object? currentRightId, object? newRightId)> GetChangedToOneRelationshipsWithForeignKeyAtRightSide()
    {
        Dictionary<HasOneAttribute, (object? currentRightId, object? newRightId)> changes = [];

        foreach ((RelationshipAttribute relationship, ISet<IIdentifiable> newRightResources) in _newRightResourcesByRelationship)
        {
            if (relationship is HasOneAttribute hasOneRelationship)
            {
                RelationshipForeignKey foreignKey = _dataModelService.GetForeignKey(hasOneRelationship);

                if (foreignKey.IsAtLeftSide)
                {
                    continue;
                }

                object? currentRightId = _currentRightResourcesByRelationship.TryGetValue(hasOneRelationship, out HashSet<IIdentifiable>? currentRightResources)
                    ? currentRightResources.FirstOrDefault()?.GetTypedId()
                    : null;

                object? newRightId = newRightResources.SingleOrDefault()?.GetTypedId();

                if (!Equals(currentRightId, newRightId))
                {
                    changes[hasOneRelationship] = (currentRightId, newRightId);
                }
            }
        }

        return changes.AsReadOnly();
    }

    public IReadOnlyDictionary<HasManyAttribute, (ISet<object> currentRightIds, ISet<object> newRightIds)> GetChangedToManyRelationships()
    {
        Dictionary<HasManyAttribute, (ISet<object> currentRightIds, ISet<object> newRightIds)> changes = [];

        foreach ((RelationshipAttribute relationship, ISet<IIdentifiable> newRightResources) in _newRightResourcesByRelationship)
        {
            if (relationship is HasManyAttribute hasManyRelationship)
            {
                HashSet<object> newRightIds = newRightResources.Select(resource => resource.GetTypedId()).ToHashSet();

                HashSet<object> currentRightIds =
                    _currentRightResourcesByRelationship.TryGetValue(hasManyRelationship, out HashSet<IIdentifiable>? currentRightResources)
                        ? currentRightResources.Select(resource => resource.GetTypedId()).ToHashSet()
                        : [];

                if (!currentRightIds.SetEquals(newRightIds))
                {
                    changes[hasManyRelationship] = (currentRightIds, newRightIds);
                }
            }
        }

        return changes.AsReadOnly();
    }

    private static void AssertSameType(ResourceType resourceType, IIdentifiable resource)
    {
        Type declaredType = resourceType.ClrType;
        Type instanceType = resource.GetClrType();

        if (instanceType != declaredType)
        {
            throw new ArgumentException($"Expected resource of type '{declaredType.Name}' instead of '{instanceType.Name}'.", nameof(resource));
        }
    }
}
