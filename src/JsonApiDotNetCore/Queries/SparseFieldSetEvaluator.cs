using System.Collections.Concurrent;
using System.Collections.Immutable;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries;

/// <inheritdoc cref="ISparseFieldSetEvaluator" />
public sealed class SparseFieldSetEvaluator : ISparseFieldSetEvaluator
{
    private static readonly ConcurrentDictionary<ResourceType, SparseFieldSetExpression> ViewableFieldSetCache = new();

    private readonly IResourceDefinitionAccessor _resourceDefinitionAccessor;
    private readonly Lazy<Dictionary<ResourceType, SparseFieldSetExpression>> _lazySourceTable;
    private readonly Dictionary<ResourceType, SparseFieldSetExpression?> _visitedQueryTable = [];
    private readonly Dictionary<ResourceType, SparseFieldSetExpression> _visitedSerializerTable = [];

    public SparseFieldSetEvaluator(IEnumerable<IQueryConstraintProvider> constraintProviders, IResourceDefinitionAccessor resourceDefinitionAccessor)
    {
        ArgumentNullException.ThrowIfNull(constraintProviders);
        ArgumentNullException.ThrowIfNull(resourceDefinitionAccessor);

        _resourceDefinitionAccessor = resourceDefinitionAccessor;
        _lazySourceTable = new Lazy<Dictionary<ResourceType, SparseFieldSetExpression>>(() => BuildSourceTable(constraintProviders));
    }

    private static Dictionary<ResourceType, SparseFieldSetExpression> BuildSourceTable(IEnumerable<IQueryConstraintProvider> constraintProviders)
    {
        var mergedTable = new Dictionary<ResourceType, SparseFieldSetExpression>();

        foreach (ResourceTypeExpressionInScope constraint in constraintProviders.SelectMany(provider => provider.GetConstraints())
            .OfType<ResourceTypeExpressionInScope>())
        {
            if (constraint.Expression is SparseFieldSetExpression sparseFieldSet)
            {
                if (mergedTable.TryGetValue(constraint.ResourceType, out SparseFieldSetExpression? existingSet))
                {
                    mergedTable[constraint.ResourceType] = UnionExpressions(existingSet, sparseFieldSet);
                }
                else
                {
                    mergedTable[constraint.ResourceType] = sparseFieldSet;
                }
            }
        }

        return mergedTable;
    }

    private static SparseFieldSetExpression UnionExpressions(SparseFieldSetExpression first, SparseFieldSetExpression second)
    {
        ImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression> mergedElements = UnionElements(first.Elements, second.Elements);

        return SparseFieldSetExpression.Create(mergedElements);
    }

    private static ImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression> UnionElements(
        IImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression> first,
        IImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression> second)
    {
        var merged = new Dictionary<ResourceFieldAttribute, SparseFieldSetElementExpression>(first);

        foreach ((ResourceFieldAttribute field, SparseFieldSetElementExpression element) in second)
        {
            if (merged.TryGetValue(field, out SparseFieldSetElementExpression? existingElement))
            {
                if (existingElement.Children.Count == 0 || element.Children.Count == 0)
                {
                    // One selects all children of this field, so the union selects all children.
                    merged[field] = new SparseFieldSetElementExpression(field);
                }
                else
                {
                    merged[field] = new SparseFieldSetElementExpression(field, UnionElements(existingElement.Children, element.Children));
                }
            }
            else
            {
                merged[field] = element;
            }
        }

        return merged.ToImmutableDictionary();
    }

    /// <inheritdoc />
    public SparseFieldSetExpression? GetSparseFieldSetForQuery(ResourceType resourceType)
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
    public SparseFieldSetExpression GetSparseFieldSetForSerializer(ResourceType resourceType)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        if (!_visitedSerializerTable.TryGetValue(resourceType, out SparseFieldSetExpression? outputExpression))
        {
            SparseFieldSetExpression inputExpression = _lazySourceTable.Value.TryGetValue(resourceType, out SparseFieldSetExpression? inputFields)
                ? inputFields
                : GetCachedViewableFieldSet(resourceType);

            SparseFieldSetExpression? callbackExpression = _resourceDefinitionAccessor.OnApplySparseFieldSet(resourceType, inputExpression);

            outputExpression = callbackExpression == null ? GetCachedViewableFieldSet(resourceType) : IntersectExpressions(inputExpression, callbackExpression);

            _visitedSerializerTable[resourceType] = outputExpression;
        }

        return outputExpression;
    }

    private static SparseFieldSetExpression IntersectExpressions(SparseFieldSetExpression first, SparseFieldSetExpression second)
    {
        ImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression> intersectedElements = IntersectElements(first.Elements, second.Elements);

        return SparseFieldSetExpression.Create(intersectedElements);
    }

    private static ImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression> IntersectElements(
        IImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression> first,
        IImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression> second)
    {
        ImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression>.Builder builder =
            ImmutableDictionary.CreateBuilder<ResourceFieldAttribute, SparseFieldSetElementExpression>();

        foreach ((ResourceFieldAttribute field, SparseFieldSetElementExpression secondElement) in second)
        {
            if (first.TryGetValue(field, out SparseFieldSetElementExpression? firstElement))
            {
                if (firstElement.Children.Count == 0)
                {
                    // first wanted all children -> restricted to second's children
                    builder.Add(field, secondElement);
                }
                else if (secondElement.Children.Count == 0)
                {
                    // second allows all children -> restricted to first's children
                    builder.Add(field, firstElement);
                }
                else
                {
                    ImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression> childIntersection =
                        IntersectElements(firstElement.Children, secondElement.Children);

                    if (childIntersection.Count > 0)
                    {
                        builder.Add(field, new SparseFieldSetElementExpression(field, childIntersection));
                    }
                }
            }
        }

        return builder.ToImmutable();
    }

    private static SparseFieldSetExpression GetCachedViewableFieldSet(ResourceType resourceType)
    {
        return ViewableFieldSetCache.GetOrAdd(resourceType, static type =>
        {
            ImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression> viewableFields = GetViewableFields(type);
            return new SparseFieldSetExpression(viewableFields);
        });
    }

    private static ImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression> GetViewableFields(ResourceType resourceType)
    {
        return resourceType.Fields.Where(nextField => !nextField.IsViewBlocked())
            .ToImmutableDictionary(field => field, field => new SparseFieldSetElementExpression(field));
    }

    public void Reset()
    {
        _visitedQueryTable.Clear();
        _visitedSerializerTable.Clear();
    }
}
