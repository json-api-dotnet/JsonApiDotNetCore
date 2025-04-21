using System.Reflection;
using DapperExample.TranslationToSql.TreeNodes;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace DapperExample.Repositories;

/// <summary>
/// Maps the result set from a SQL query that includes primary and related resources.
/// </summary>
internal sealed class ResultSetMapper<TResource, TId>
    where TResource : class, IIdentifiable<TId>
{
    private readonly List<Type> _joinObjectTypes = [];

    // For each object type, we keep a map of ID/instance pairs.
    // Note we don't do full bidirectional relationship fix-up; this just avoids duplicate instances.
    private readonly Dictionary<Type, Dictionary<object, object>> _resourceByTypeCache = [];

    // Used to determine where in the tree of included relationships a join object belongs to.
    private readonly Dictionary<IncludeElementExpression, int> _includeElementToJoinObjectArrayIndexLookup = new(ReferenceEqualityComparer.Instance);

    // The return value of the mapping process.
    private readonly List<TResource> _primaryResourcesInOrder = [];

    // The included relationships for which an INNER/LEFT JOIN statement was produced, which we're mapping.
    private readonly IncludeExpression _include;

    public Type[] ResourceClrTypes => _joinObjectTypes.ToArray();

    public ResultSetMapper(IncludeExpression? include)
    {
        _include = include ?? IncludeExpression.Empty;
        _joinObjectTypes.Add(typeof(TResource));
        _resourceByTypeCache[typeof(TResource)] = [];

        var walker = new IncludeElementWalker(_include);
        int index = 1;

        foreach (IncludeElementExpression includeElement in walker.BreadthFirstEnumerate())
        {
            _joinObjectTypes.Add(includeElement.Relationship.RightType.ClrType);
            _resourceByTypeCache[includeElement.Relationship.RightType.ClrType] = [];
            _includeElementToJoinObjectArrayIndexLookup[includeElement] = index;

            index++;
        }
    }

    public object? Map(object[] joinObjects)
    {
        // This method executes for each row in the SQL result set.

        if (joinObjects.Length != _includeElementToJoinObjectArrayIndexLookup.Count + 1)
        {
            throw new InvalidOperationException("Failed to properly map SQL result set into objects.");
        }

        object?[] objectsCached = joinObjects.Select(GetCached).ToArray();
        var leftResource = (TResource?)objectsCached[0];

        if (leftResource == null)
        {
            throw new InvalidOperationException("Failed to properly map SQL result set into objects.");
        }

        RecursiveSetRelationships(leftResource, _include.Elements, objectsCached);

        _primaryResourcesInOrder.Add(leftResource);
        return null;
    }

    private object? GetCached(object? resource)
    {
        if (resource == null)
        {
            return null;
        }

        object? resourceId = GetResourceId(resource);

        if (resourceId == null || HasDefaultValue(resourceId))
        {
            // When Id is not set, the entire object is empty (due to LEFT JOIN usage).
            return null;
        }

        Dictionary<object, object> resourceByIdCache = _resourceByTypeCache[resource.GetType()];

        if (resourceByIdCache.TryGetValue(resourceId, out object? cachedValue))
        {
            return cachedValue;
        }

        resourceByIdCache[resourceId] = resource;
        return resource;
    }

    private static object? GetResourceId(object resource)
    {
        PropertyInfo? property = resource.GetType().GetProperty(TableSourceNode.IdColumnName);

        if (property == null)
        {
            throw new InvalidOperationException($"{TableSourceNode.IdColumnName} property not found on object of type '{resource.GetType().Name}'.");
        }

        return property.GetValue(resource);
    }

    private bool HasDefaultValue(object value)
    {
        object? defaultValue = RuntimeTypeConverter.GetDefaultValue(value.GetType());
        return Equals(defaultValue, value);
    }

    private void RecursiveSetRelationships(object leftResource, IEnumerable<IncludeElementExpression> includeElements, object?[] joinObjects)
    {
        foreach (IncludeElementExpression includeElement in includeElements)
        {
            int rightIndex = _includeElementToJoinObjectArrayIndexLookup[includeElement];
            object? rightResource = joinObjects[rightIndex];

            SetRelationship(leftResource, includeElement.Relationship, rightResource);

            if (rightResource != null && includeElement.Children.Count > 0)
            {
                RecursiveSetRelationships(rightResource, includeElement.Children, joinObjects);
            }
        }
    }

    private void SetRelationship(object leftResource, RelationshipAttribute relationship, object? rightResource)
    {
        if (rightResource != null)
        {
            if (relationship is HasManyAttribute hasManyRelationship)
            {
                hasManyRelationship.AddValue(leftResource, (IIdentifiable)rightResource);
            }
            else
            {
                relationship.SetValue(leftResource, rightResource);
            }
        }
    }

    public IReadOnlyCollection<TResource> GetResources()
    {
        return _primaryResourcesInOrder.DistinctBy(resource => resource.Id).ToArray().AsReadOnly();
    }

    private sealed class IncludeElementWalker(IncludeExpression include)
    {
        private readonly IncludeExpression _include = include;

        public IEnumerable<IncludeElementExpression> BreadthFirstEnumerate()
        {
            foreach (IncludeElementExpression next in _include.Elements.OrderBy(element => element.Relationship.PublicName)
                .SelectMany(RecursiveEnumerateElement))
            {
                yield return next;
            }
        }

        private IEnumerable<IncludeElementExpression> RecursiveEnumerateElement(IncludeElementExpression element)
        {
            yield return element;

            foreach (IncludeElementExpression next in element.Children.OrderBy(child => child.Relationship.PublicName).SelectMany(RecursiveEnumerateElement))
            {
                yield return next;
            }
        }
    }
}
