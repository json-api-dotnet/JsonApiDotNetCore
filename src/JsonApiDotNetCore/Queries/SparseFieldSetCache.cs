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
    private readonly Lazy<Dictionary<ResourceType, ImmutableHashSet<ResourceFieldChainExpression>>> _lazySourceTable;
    private readonly Dictionary<ResourceType, IImmutableSet<ResourceFieldChainExpression>> _visitedTable = [];

    public SparseFieldSetCache(IEnumerable<IQueryConstraintProvider> constraintProviders, IResourceDefinitionAccessor resourceDefinitionAccessor)
    {
        ArgumentNullException.ThrowIfNull(constraintProviders);
        ArgumentNullException.ThrowIfNull(resourceDefinitionAccessor);

        _resourceDefinitionAccessor = resourceDefinitionAccessor;
        _lazySourceTable = new Lazy<Dictionary<ResourceType, ImmutableHashSet<ResourceFieldChainExpression>>>(() => BuildSourceTable(constraintProviders));
    }

    private static Dictionary<ResourceType, ImmutableHashSet<ResourceFieldChainExpression>> BuildSourceTable(
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

            AddSparseFieldsToSet(sparseFieldSet.Fields, builder);
        }

        return mergedTable.ToDictionary(pair => pair.Key, pair => pair.Value.ToImmutable());
    }

    private static void AddSparseFieldsToSet(IImmutableSet<ResourceFieldChainExpression> sparseFieldsToAdd,
        ImmutableHashSet<ResourceFieldChainExpression>.Builder sparseFieldSetBuilder)
    {
        foreach (ResourceFieldChainExpression field in sparseFieldsToAdd)
        {
            sparseFieldSetBuilder.Add(field);
        }
    }

    /// <inheritdoc />
    public IImmutableSet<ResourceFieldChainExpression> GetSparseFieldSetForQuery(ResourceType resourceType)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        if (!_visitedTable.TryGetValue(resourceType, out IImmutableSet<ResourceFieldChainExpression>? outputFields))
        {
            SparseFieldSetExpression? inputExpression =
                _lazySourceTable.Value.TryGetValue(resourceType, out ImmutableHashSet<ResourceFieldChainExpression>? inputFields)
                    ? new SparseFieldSetExpression(inputFields)
                    : null;

            SparseFieldSetExpression? outputExpression = _resourceDefinitionAccessor.OnApplySparseFieldSet(resourceType, inputExpression);
            outputFields = outputExpression == null ? ImmutableHashSet<ResourceFieldChainExpression>.Empty : outputExpression.Fields;

            _visitedTable[resourceType] = outputFields;
        }

        return outputFields;
    }

    /// <inheritdoc />
    public IImmutableSet<ResourceFieldChainExpression> GetIdAttributeSetForRelationshipQuery(ResourceType resourceType)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        AttrAttribute idAttribute = resourceType.GetAttributeByPropertyName(nameof(Identifiable<object>.Id));
        var inputExpression = new SparseFieldSetExpression(ImmutableHashSet.Create(new ResourceFieldChainExpression(idAttribute)));

        // Intentionally not cached, as we are fetching ID only (ignoring any sparse fieldset that came from query string).
        SparseFieldSetExpression? outputExpression = _resourceDefinitionAccessor.OnApplySparseFieldSet(resourceType, inputExpression);

        ImmutableHashSet<ResourceFieldChainExpression> outputAttributes = outputExpression == null
            ? ImmutableHashSet<ResourceFieldChainExpression>.Empty
            : outputExpression.Fields.Where(field=>field.Fields[0] is AttrAttribute).ToImmutableHashSet();

        outputAttributes = outputAttributes.Add(new ResourceFieldChainExpression(idAttribute));
        return outputAttributes;
    }

    /// <inheritdoc />
    public IImmutableSet<ResourceFieldChainExpression> GetSparseFieldSetForSerializer(ResourceType resourceType)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        if (!_visitedTable.TryGetValue(resourceType, out IImmutableSet<ResourceFieldChainExpression>? outputFields))
        {
            SparseFieldSetExpression inputExpression =
                _lazySourceTable.Value.TryGetValue(resourceType, out ImmutableHashSet<ResourceFieldChainExpression>? inputFields)
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
            ImmutableHashSet<ResourceFieldChainExpression> viewableFields = GetViewableFields(resourceType);
            fieldSet = new SparseFieldSetExpression(viewableFields);
            ViewableFieldSetCache[resourceType] = fieldSet;
        }

        return fieldSet;
    }

    private static ImmutableHashSet<ResourceFieldChainExpression> GetViewableFields(ResourceType resourceType)
    {
        return resourceType.Fields.Where(nextField => !nextField.IsViewBlocked()).Select(nextField => new ResourceFieldChainExpression(nextField))
            .ToImmutableHashSet();
    }

    public void Reset()
    {
        _visitedTable.Clear();
    }
}
