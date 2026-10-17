using System.Collections.Concurrent;
using System.Collections.Immutable;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries;

/// <inheritdoc cref="ISparseFieldSetCache" />
public sealed class SparseFieldSetCache : ISparseFieldSetCache
{
    private static readonly ConcurrentDictionary<ResourceType, SparseFieldSetExpression> ViewableFieldSetCache = new();

    private readonly IResourceDefinitionAccessor _resourceDefinitionAccessor;
    private readonly Lazy<Dictionary<ResourceType, SparseFieldSetExpression>> _lazySourceTable;
    private readonly Dictionary<ResourceType, SparseFieldSetExpression?> _visitedQueryTable = [];
    private readonly Dictionary<ResourceType, SparseFieldSetExpression> _visitedSerializerTable = [];

    public SparseFieldSetCache(IEnumerable<IQueryConstraintProvider> constraintProviders, IResourceDefinitionAccessor resourceDefinitionAccessor)
    {
        ArgumentNullException.ThrowIfNull(constraintProviders);
        ArgumentNullException.ThrowIfNull(resourceDefinitionAccessor);

        _resourceDefinitionAccessor = resourceDefinitionAccessor;
        _lazySourceTable = new Lazy<Dictionary<ResourceType, SparseFieldSetExpression>>(() => BuildSourceTable(constraintProviders));
    }

    private static Dictionary<ResourceType, SparseFieldSetExpression> BuildSourceTable(
        IEnumerable<IQueryConstraintProvider> constraintProviders)
    {
        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:wrap_before_first_method_call true

        KeyValuePair<ResourceType, SparseFieldSetExpression>[] sparseFieldTables = constraintProviders
            .SelectMany(provider => provider.GetConstraints())
            .Where(constraint => constraint.Scope == null)
            .Select(constraint => constraint.Expression)
            .OfType<SparseFieldTableExpression>()
            .Select(expression => expression.Table)
            .SelectMany(table => table)
            .ToArray();

        // @formatter:wrap_before_first_method_call restore
        // @formatter:wrap_chained_method_calls restore

        var mergedTable = new Dictionary<ResourceType, ImmutableHashSet<ResourceFieldChainExpression>.Builder>();

        foreach ((ResourceType resourceType, SparseFieldSetExpression sparseFieldSet) in sparseFieldTables)
        {
            if (!mergedTable.TryGetValue(resourceType, out ImmutableHashSet<ResourceFieldChainExpression>.Builder? builder))
            {
                builder = ImmutableHashSet.CreateBuilder<ResourceFieldChainExpression>();
                mergedTable[resourceType] = builder;
            }

            foreach (ResourceFieldChainExpression chain in sparseFieldSet.FieldChains)
            {
                builder.Add(chain);
            }
        }

        return mergedTable.ToDictionary(pair => pair.Key, pair => new SparseFieldSetExpression(pair.Value.ToImmutable()));
    }

    /// <inheritdoc />
    public SparseFieldSetExpression? GetSparseFieldSetExpressionForQuery(ResourceType resourceType)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        if (!_visitedQueryTable.TryGetValue(resourceType, out SparseFieldSetExpression? outputExpression))
        {
            SparseFieldSetExpression? inputExpression = _lazySourceTable.Value.GetValueOrDefault(resourceType);

            outputExpression = _resourceDefinitionAccessor.OnApplySparseFieldSet(resourceType, inputExpression);
            _visitedQueryTable[resourceType] = outputExpression;
        }

        return outputExpression;
    }

    /// <inheritdoc />
    public SparseFieldSetExpression GetSparseFieldSetExpressionForSerializer(ResourceType resourceType)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        if (!_visitedSerializerTable.TryGetValue(resourceType, out SparseFieldSetExpression? outputExpression))
        {
            SparseFieldSetExpression inputExpression =
                _lazySourceTable.Value.TryGetValue(resourceType, out SparseFieldSetExpression? inputFields)
                    ? inputFields
                    : GetCachedViewableFieldSet(resourceType);

            SparseFieldSetExpression? callbackExpression = _resourceDefinitionAccessor.OnApplySparseFieldSet(resourceType, inputExpression);

            outputExpression = callbackExpression == null
                ? GetCachedViewableFieldSet(resourceType)
                : IntersectExpressions(inputExpression, callbackExpression);

            _visitedSerializerTable[resourceType] = outputExpression;
        }

        return outputExpression;
    }

    private static SparseFieldSetExpression IntersectExpressions(SparseFieldSetExpression first, SparseFieldSetExpression second)
    {
        IImmutableSet<ResourceFieldChainExpression> intersectedChains = first.FieldChains.Intersect(second.FieldChains);
        return intersectedChains.Count == 0 ? first : new SparseFieldSetExpression(intersectedChains);
    }

    /// <inheritdoc />
    public IImmutableSet<ResourceFieldAttribute> GetSparseFieldSetForQuery(ResourceType resourceType)
    {
        SparseFieldSetExpression? expression = GetSparseFieldSetExpressionForQuery(resourceType);
        return expression == null ? ImmutableHashSet<ResourceFieldAttribute>.Empty : expression.Fields;
    }

    /// <inheritdoc />
    public IImmutableSet<AttrAttribute> GetIdAttributeSetForRelationshipQuery(ResourceType resourceType)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        AttrAttribute idAttribute = resourceType.GetAttributeByPropertyName(nameof(Identifiable<>.Id));
        var inputExpression = new SparseFieldSetExpression(ImmutableHashSet.Create<ResourceFieldAttribute>(idAttribute));

        // Intentionally not cached, as we are fetching ID only (ignoring any sparse fieldset that came from query string).
        SparseFieldSetExpression? outputExpression = _resourceDefinitionAccessor.OnApplySparseFieldSet(resourceType, inputExpression);

        ImmutableHashSet<AttrAttribute> outputAttributes = outputExpression == null
            ? ImmutableHashSet<AttrAttribute>.Empty
            : outputExpression.Fields.OfType<AttrAttribute>().ToImmutableHashSet();

        outputAttributes = outputAttributes.Add(idAttribute);
        return outputAttributes;
    }

    /// <inheritdoc />
    public IImmutableSet<ResourceFieldAttribute> GetSparseFieldSetForSerializer(ResourceType resourceType)
    {
        return GetSparseFieldSetExpressionForSerializer(resourceType).Fields;
    }

    private static SparseFieldSetExpression GetCachedViewableFieldSet(ResourceType resourceType)
    {
        if (!ViewableFieldSetCache.TryGetValue(resourceType, out SparseFieldSetExpression? fieldSet))
        {
            ImmutableHashSet<ResourceFieldAttribute> viewableFields = GetViewableFields(resourceType);
            fieldSet = new SparseFieldSetExpression(viewableFields);
            ViewableFieldSetCache[resourceType] = fieldSet;
        }

        return fieldSet;
    }

    private static ImmutableHashSet<ResourceFieldAttribute> GetViewableFields(ResourceType resourceType)
    {
        return resourceType.Fields.Where(nextField => !nextField.IsViewBlocked()).ToImmutableHashSet();
    }

    public void Reset()
    {
        _visitedQueryTable.Clear();
        _visitedSerializerTable.Clear();
    }
}
