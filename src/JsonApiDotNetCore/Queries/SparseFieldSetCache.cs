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
    private readonly Lazy<Dictionary<ResourceType, ImmutableHashSet<ResourceFieldAttribute>>> _lazySourceTable;
    private readonly Dictionary<ResourceType, IImmutableSet<ResourceFieldAttribute>> _visitedTable = [];

    public SparseFieldSetCache(IEnumerable<IQueryConstraintProvider> constraintProviders, IResourceDefinitionAccessor resourceDefinitionAccessor)
    {
        ArgumentGuard.NotNull(constraintProviders);
        ArgumentGuard.NotNull(resourceDefinitionAccessor);

        _resourceDefinitionAccessor = resourceDefinitionAccessor;
        _lazySourceTable = new Lazy<Dictionary<ResourceType, ImmutableHashSet<ResourceFieldAttribute>>>(() => BuildSourceTable(constraintProviders));
    }

    private static Dictionary<ResourceType, ImmutableHashSet<ResourceFieldAttribute>> BuildSourceTable(
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

        var mergedTable = new Dictionary<ResourceType, ImmutableHashSet<ResourceFieldAttribute>.Builder>();

        foreach ((ResourceType resourceType, SparseFieldSetExpression sparseFieldSet) in sparseFieldTables)
        {
            if (!mergedTable.TryGetValue(resourceType, out ImmutableHashSet<ResourceFieldAttribute>.Builder? builder))
            {
                builder = ImmutableHashSet.CreateBuilder<ResourceFieldAttribute>();
                mergedTable[resourceType] = builder;
            }

            AddSparseFieldsToSet(sparseFieldSet.Fields, builder);
        }

        return mergedTable.ToDictionary(pair => pair.Key, pair => pair.Value.ToImmutable());
    }

    private static void AddSparseFieldsToSet(IImmutableSet<ResourceFieldAttribute> sparseFieldsToAdd,
        ImmutableHashSet<ResourceFieldAttribute>.Builder sparseFieldSetBuilder)
    {
        foreach (ResourceFieldAttribute field in sparseFieldsToAdd)
        {
            sparseFieldSetBuilder.Add(field);
        }
    }

    /// <inheritdoc />
    public IImmutableSet<ResourceFieldAttribute> GetSparseFieldSetForQuery(ResourceType resourceType)
    {
        ArgumentGuard.NotNull(resourceType);

        if (!_visitedTable.TryGetValue(resourceType, out IImmutableSet<ResourceFieldAttribute>? outputFields))
        {
            SparseFieldSetExpression? inputExpression =
                _lazySourceTable.Value.TryGetValue(resourceType, out ImmutableHashSet<ResourceFieldAttribute>? inputFields)
                    ? new SparseFieldSetExpression(inputFields)
                    : null;

            SparseFieldSetExpression? outputExpression = _resourceDefinitionAccessor.OnApplySparseFieldSet(resourceType, inputExpression);
            outputFields = outputExpression == null ? ImmutableHashSet<ResourceFieldAttribute>.Empty : outputExpression.Fields;

            _visitedTable[resourceType] = outputFields;
        }

        return outputFields;
    }

    /// <inheritdoc />
    public IImmutableSet<AttrAttribute> GetIdAttributeSetForRelationshipQuery(ResourceType resourceType)
    {
        ArgumentGuard.NotNull(resourceType);

        AttrAttribute idAttribute = resourceType.GetAttributeByPropertyName(nameof(Identifiable<object>.Id));
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
        ArgumentGuard.NotNull(resourceType);

        if (!_visitedTable.TryGetValue(resourceType, out IImmutableSet<ResourceFieldAttribute>? outputFields))
        {
            SparseFieldSetExpression inputExpression =
                _lazySourceTable.Value.TryGetValue(resourceType, out ImmutableHashSet<ResourceFieldAttribute>? inputFields)
                    ? new SparseFieldSetExpression(inputFields)
                    : GetCachedViewableFieldSet(resourceType);

            SparseFieldSetExpression? outputExpression = _resourceDefinitionAccessor.OnApplySparseFieldSet(resourceType, inputExpression);

            outputFields = outputExpression == null
                ? GetCachedViewableFieldSet(resourceType).Fields
                : inputExpression.Fields.Intersect(outputExpression.Fields);

            _visitedTable[resourceType] = outputFields;
        }

        return outputFields;
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
        _visitedTable.Clear();
    }
}
