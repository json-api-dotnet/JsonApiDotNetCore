using System.Collections.Concurrent;
using System.Collections.Immutable;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Internal;

/// <inheritdoc />
public sealed class SparseFieldSetCache : ISparseFieldSetCache
{
    private static readonly ConcurrentDictionary<ResourceType, SparseFieldSetExpression> ViewableFieldSetCache = new();

    private readonly IResourceDefinitionAccessor _resourceDefinitionAccessor;
    private readonly Lazy<IDictionary<ResourceType, IImmutableSet<ResourceFieldAttribute>>> _lazySourceTable;
    private readonly IDictionary<ResourceType, IImmutableSet<ResourceFieldAttribute>> _visitedTable;

    public SparseFieldSetCache(IEnumerable<IQueryConstraintProvider> constraintProviders, IResourceDefinitionAccessor resourceDefinitionAccessor)
    {
        ArgumentGuard.NotNull(constraintProviders);
        ArgumentGuard.NotNull(resourceDefinitionAccessor);

        _resourceDefinitionAccessor = resourceDefinitionAccessor;
        _lazySourceTable = new Lazy<IDictionary<ResourceType, IImmutableSet<ResourceFieldAttribute>>>(() => BuildSourceTable(constraintProviders));
        _visitedTable = new Dictionary<ResourceType, IImmutableSet<ResourceFieldAttribute>>();
    }

    private static IDictionary<ResourceType, IImmutableSet<ResourceFieldAttribute>> BuildSourceTable(IEnumerable<IQueryConstraintProvider> constraintProviders)
    {
        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:keep_existing_linebreaks true

        KeyValuePair<ResourceType, SparseFieldSetExpression>[] sparseFieldTables = constraintProviders
            .SelectMany(provider => provider.GetConstraints())
            .Where(constraint => constraint.Scope == null)
            .Select(constraint => constraint.Expression)
            .OfType<SparseFieldTableExpression>()
            .Select(expression => expression.Table)
            .SelectMany(table => table)
            .ToArray();

        // @formatter:keep_existing_linebreaks restore
        // @formatter:wrap_chained_method_calls restore

        var mergedTable = new Dictionary<ResourceType, ImmutableHashSet<ResourceFieldAttribute>.Builder>();

        foreach ((ResourceType resourceType, SparseFieldSetExpression sparseFieldSet) in sparseFieldTables)
        {
            if (!mergedTable.ContainsKey(resourceType))
            {
                mergedTable[resourceType] = ImmutableHashSet.CreateBuilder<ResourceFieldAttribute>();
            }

            AddSparseFieldsToSet(sparseFieldSet.Fields, mergedTable[resourceType]);
        }

        return mergedTable.ToDictionary(pair => pair.Key, pair => (IImmutableSet<ResourceFieldAttribute>)pair.Value.ToImmutable());
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

        if (!_visitedTable.ContainsKey(resourceType))
        {
            SparseFieldSetExpression? inputExpression = _lazySourceTable.Value.TryGetValue(resourceType, out IImmutableSet<ResourceFieldAttribute>? inputFields)
                ? new SparseFieldSetExpression(inputFields)
                : null;

            SparseFieldSetExpression? outputExpression = _resourceDefinitionAccessor.OnApplySparseFieldSet(resourceType, inputExpression);

            IImmutableSet<ResourceFieldAttribute> outputFields = outputExpression == null
                ? ImmutableHashSet<ResourceFieldAttribute>.Empty
                : outputExpression.Fields;

            _visitedTable[resourceType] = outputFields;
        }

        return _visitedTable[resourceType];
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

        if (!_visitedTable.ContainsKey(resourceType))
        {
            SparseFieldSetExpression inputExpression = _lazySourceTable.Value.TryGetValue(resourceType, out IImmutableSet<ResourceFieldAttribute>? inputFields)
                ? new SparseFieldSetExpression(inputFields)
                : GetCachedViewableFieldSet(resourceType);

            SparseFieldSetExpression? outputExpression = _resourceDefinitionAccessor.OnApplySparseFieldSet(resourceType, inputExpression);

            IImmutableSet<ResourceFieldAttribute> outputFields = outputExpression == null
                ? GetCachedViewableFieldSet(resourceType).Fields
                : inputExpression.Fields.Intersect(outputExpression.Fields);

            _visitedTable[resourceType] = outputFields;
        }

        return _visitedTable[resourceType];
    }

    private static SparseFieldSetExpression GetCachedViewableFieldSet(ResourceType resourceType)
    {
        if (!ViewableFieldSetCache.TryGetValue(resourceType, out SparseFieldSetExpression? fieldSet))
        {
            IImmutableSet<ResourceFieldAttribute> viewableFields = GetViewableFields(resourceType);
            fieldSet = new SparseFieldSetExpression(viewableFields);
            ViewableFieldSetCache[resourceType] = fieldSet;
        }

        return fieldSet;
    }

    private static IImmutableSet<ResourceFieldAttribute> GetViewableFields(ResourceType resourceType)
    {
        ImmutableHashSet<ResourceFieldAttribute>.Builder fieldSetBuilder = ImmutableHashSet.CreateBuilder<ResourceFieldAttribute>();

        foreach (ResourceFieldAttribute field in resourceType.Fields.Where(nextField => !nextField.IsViewBlocked()))
        {
            fieldSetBuilder.Add(field);
        }

        return fieldSetBuilder.ToImmutable();
    }

    public void Reset()
    {
        _visitedTable.Clear();
    }
}
