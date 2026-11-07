using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Diagnostics;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries;

/// <inheritdoc cref="IQueryLayerComposer" />
[PublicAPI]
public class QueryLayerComposer : IQueryLayerComposer
{
    private readonly IQueryConstraintProvider[] _constraintProviders;
    private readonly IResourceDefinitionAccessor _resourceDefinitionAccessor;
    private readonly IJsonApiOptions _options;
    private readonly IPaginationContext _paginationContext;
    private readonly ITargetedFields _targetedFields;
    private readonly IEvaluatedIncludeCache _evaluatedIncludeCache;
    private readonly ISparseFieldSetEvaluator _sparseFieldSetEvaluator;

    public QueryLayerComposer(IEnumerable<IQueryConstraintProvider> constraintProviders, IResourceDefinitionAccessor resourceDefinitionAccessor,
        IJsonApiOptions options, IPaginationContext paginationContext, ITargetedFields targetedFields, IEvaluatedIncludeCache evaluatedIncludeCache,
        ISparseFieldSetEvaluator sparseFieldSetEvaluator)
    {
        ArgumentNullException.ThrowIfNull(constraintProviders);
        ArgumentNullException.ThrowIfNull(resourceDefinitionAccessor);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(paginationContext);
        ArgumentNullException.ThrowIfNull(targetedFields);
        ArgumentNullException.ThrowIfNull(evaluatedIncludeCache);
        ArgumentNullException.ThrowIfNull(sparseFieldSetEvaluator);

        _constraintProviders = constraintProviders as IQueryConstraintProvider[] ?? constraintProviders.ToArray();
        _resourceDefinitionAccessor = resourceDefinitionAccessor;
        _options = options;
        _paginationContext = paginationContext;
        _targetedFields = targetedFields;
        _evaluatedIncludeCache = evaluatedIncludeCache;
        _sparseFieldSetEvaluator = sparseFieldSetEvaluator;
    }

    /// <inheritdoc />
    public FilterExpression? GetSecondaryFilterFromConstraints<TId>([DisallowNull] TId primaryId, HasManyAttribute hasManyRelationship)
    {
        ArgumentNullException.ThrowIfNull(primaryId);
        ArgumentNullException.ThrowIfNull(hasManyRelationship);

        if (hasManyRelationship.InverseNavigationProperty == null)
        {
            return null;
        }

        RelationshipAttribute? inverseRelationship =
            hasManyRelationship.RightType.FindRelationshipByPropertyName(hasManyRelationship.InverseNavigationProperty.Name);

        if (inverseRelationship == null)
        {
            return null;
        }

        ImmutableArray<ExpressionInScope> constraints = [.. _constraintProviders.SelectMany(provider => provider.GetConstraints())];
        ReadOnlyCollection<QueryExpression> expressionsInSecondaryScope = GetExpressionsInTopScope(constraints);

        FilterExpression? primaryFilter = GetFilterFromResourceDefinition(hasManyRelationship.LeftType);

        if (primaryFilter != null && inverseRelationship is HasOneAttribute)
        {
            // We can't lift the field chains in a primary filter, because there's no way for a custom filter expression to express
            // the scope of its chains. See https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/1671.
            return null;
        }

        FilterExpression? secondaryFilter = GetFilter(expressionsInSecondaryScope, hasManyRelationship.RightType);
        FilterExpression inverseFilter = GetInverseRelationshipFilter(primaryId, primaryFilter, hasManyRelationship, inverseRelationship);

        return LogicalExpression.Compose(LogicalOperator.And, inverseFilter, secondaryFilter);
    }

    private static FilterExpression GetInverseRelationshipFilter<TId>([DisallowNull] TId primaryId, FilterExpression? primaryFilter,
        HasManyAttribute relationship, RelationshipAttribute inverseRelationship)
    {
        return inverseRelationship is HasManyAttribute hasManyInverseRelationship
            ? GetInverseHasManyRelationshipFilter(primaryId, primaryFilter, relationship, hasManyInverseRelationship)
            : GetInverseHasOneRelationshipFilter(primaryId, relationship, (HasOneAttribute)inverseRelationship);
    }

    private static ComparisonExpression GetInverseHasOneRelationshipFilter<TId>([DisallowNull] TId primaryId, HasManyAttribute relationship,
        HasOneAttribute inverseRelationship)
    {
        AttrAttribute idAttribute = GetIdAttribute(relationship.LeftType);
        var idChain = new ResourceFieldChainExpression(ImmutableArray.Create<ResourceFieldAttribute>(inverseRelationship, idAttribute));

        return new ComparisonExpression(ComparisonOperator.Equals, idChain, new LiteralConstantExpression(primaryId));
    }

    private static HasExpression GetInverseHasManyRelationshipFilter<TId>([DisallowNull] TId primaryId, FilterExpression? primaryFilter,
        HasManyAttribute relationship, HasManyAttribute inverseRelationship)
    {
        FilterExpression filter = CreateFilterForIds(relationship.LeftType, primaryFilter, primaryId)!;
        return new HasExpression(new ResourceFieldChainExpression(inverseRelationship), filter);
    }

    /// <inheritdoc />
    public QueryLayer ComposeFromConstraints(ResourceType requestResourceType)
    {
        ArgumentNullException.ThrowIfNull(requestResourceType);

        ImmutableArray<ExpressionInScope> constraints = [.. _constraintProviders.SelectMany(provider => provider.GetConstraints())];

        QueryLayer topLayer = ComposeTopLayer(constraints, requestResourceType);
        topLayer.Include = ComposeChildren(topLayer, constraints);

        return topLayer;
    }

    private QueryLayer ComposeTopLayer(ImmutableArray<ExpressionInScope> constraints, ResourceType resourceType)
    {
        using IDisposable _ = CodeTimingSessionManager.Current.Measure("Top-level query composition");

        ReadOnlyCollection<QueryExpression> expressionsInTopScope = GetExpressionsInTopScope(constraints);

        PaginationExpression topPagination = GetTopPagination(expressionsInTopScope, resourceType);

        var topLayer = new QueryLayer(resourceType)
        {
            Filter = GetFilter(expressionsInTopScope, resourceType),
            Sort = GetSort(expressionsInTopScope, resourceType),
            Pagination = topPagination,
            Selection = GetSelectionForSparseAttributeSet(resourceType)
        };

        EnsureDeterministicSort(topLayer);

        return topLayer;
    }

    private static ReadOnlyCollection<QueryExpression> GetExpressionsInTopScope(ImmutableArray<ExpressionInScope> constraints)
    {
        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:wrap_before_first_method_call true

        return constraints
            .Where(constraint => constraint is not RelationshipChainExpressionInScope chainScope || chainScope.Scope == null)
            .Select(constraint => constraint.Expression)
            .ToArray()
            .AsReadOnly();

        // @formatter:wrap_before_first_method_call restore
        // @formatter:wrap_chained_method_calls restore
    }

    private PaginationExpression GetTopPagination(IReadOnlyCollection<QueryExpression> expressionsInTopScope, ResourceType resourceType)
    {
        PaginationExpression topPagination = GetPagination(expressionsInTopScope, resourceType);

        _paginationContext.PageSize = topPagination.PageSize;
        _paginationContext.PageNumber = topPagination.PageNumber;

        return topPagination;
    }

    private void EnsureDeterministicSort(QueryLayer layer)
    {
        if (layer is { Pagination.PageSize: not null, Sort: null })
        {
            layer.Sort = CreateSortById(layer.ResourceType);
        }
    }

    private IncludeExpression ComposeChildren(QueryLayer topLayer, ImmutableArray<ExpressionInScope> constraints)
    {
        using IDisposable _ = CodeTimingSessionManager.Current.Measure("Nested query composition");

        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:wrap_before_first_method_call true

        IncludeExpression include = constraints
            .OfType<RelationshipChainExpressionInScope>()
            .Where(constraint => constraint.Scope == null)
            .Select(constraint => constraint.Expression)
            .OfType<IncludeExpression>()
            .FirstOrDefault() ?? IncludeExpression.Empty;

        // @formatter:wrap_before_first_method_call restore
        // @formatter:wrap_chained_method_calls restore

        IImmutableSet<IncludeElementExpression> includeElements =
            ProcessIncludeSet(include.Elements, topLayer, ImmutableArray<RelationshipAttribute>.Empty, constraints);

        IncludeExpression result = !ReferenceEquals(includeElements, include.Elements) ? IncludeExpression.Create(includeElements) : include;

        _evaluatedIncludeCache.Set(result);

        return result;
    }

    private IImmutableSet<IncludeElementExpression> ProcessIncludeSet(IImmutableSet<IncludeElementExpression> includeElements, QueryLayer parentLayer,
        ImmutableArray<RelationshipAttribute> parentRelationshipChain, ImmutableArray<ExpressionInScope> constraints)
    {
        IImmutableSet<IncludeElementExpression> includeElementsEvaluated = GetIncludeElements(includeElements, parentLayer.ResourceType);

        var updatesInChildren = new Dictionary<IncludeElementExpression, IImmutableSet<IncludeElementExpression>>();

        foreach (IncludeElementExpression includeElement in includeElementsEvaluated)
        {
            parentLayer.Selection ??= new FieldSelection();
            FieldSelectors selectors = parentLayer.Selection.GetOrCreateSelectors(includeElement.Relationship.LeftType);

            if (!selectors.ContainsField(includeElement.Relationship))
            {
                ImmutableArray<RelationshipAttribute> relationshipChain = parentRelationshipChain.Add(includeElement.Relationship);

                // @formatter:wrap_chained_method_calls chop_always
                // @formatter:wrap_before_first_method_call true

                ReadOnlyCollection<QueryExpression> expressionsInCurrentScope = constraints
                    .OfType<RelationshipChainExpressionInScope>()
                    .Where(constraint => constraint.Scope != null && constraint.Scope.Fields.SequenceEqual(relationshipChain))
                    .Select(constraint => constraint.Expression)
                    .ToArray()
                    .AsReadOnly();

                // @formatter:wrap_before_first_method_call restore
                // @formatter:wrap_chained_method_calls restore

                ResourceType resourceType = includeElement.Relationship.RightType;
                bool isToManyRelationship = includeElement.Relationship is HasManyAttribute;
                bool allowPagination = includeElement.Relationship is HasManyAttribute { DisablePagination: false };

                var subLayer = new QueryLayer(resourceType)
                {
                    Filter = isToManyRelationship ? GetFilter(expressionsInCurrentScope, resourceType) : null,
                    Sort = isToManyRelationship ? GetSort(expressionsInCurrentScope, resourceType) : null,
                    Pagination = allowPagination ? GetPagination(expressionsInCurrentScope, resourceType) : null,
                    Selection = GetSelectionForSparseAttributeSet(resourceType)
                };

                if (subLayer is { Pagination.PageSize: not null, Sort: null })
                {
                    subLayer.Sort = CreateSortById(resourceType);
                }

                selectors.IncludeRelationship(includeElement.Relationship, subLayer);

                IImmutableSet<IncludeElementExpression> updatedChildren = ProcessIncludeSet(includeElement.Children, subLayer, relationshipChain, constraints);

                if (!ReferenceEquals(includeElement.Children, updatedChildren))
                {
                    updatesInChildren.Add(includeElement, updatedChildren);
                }
            }
        }

        EliminateRedundantSelectors(parentLayer);

        return updatesInChildren.Count == 0 ? includeElementsEvaluated : ApplyIncludeElementUpdates(includeElementsEvaluated, updatesInChildren);
    }

    private static void EliminateRedundantSelectors(QueryLayer parentLayer)
    {
        if (parentLayer.Selection != null)
        {
            foreach ((ResourceType resourceType, FieldSelectors selectors) in parentLayer.Selection.ToArray())
            {
                if (selectors.ContainsOnlyRelationships && selectors.Values.OfType<QueryLayer>().All(subLayer => subLayer.IsEmpty))
                {
                    parentLayer.Selection.Remove(resourceType);
                }
            }

            if (parentLayer.Selection.IsEmpty)
            {
                parentLayer.Selection = null;
            }
        }
    }

    private static ImmutableHashSet<IncludeElementExpression> ApplyIncludeElementUpdates(IImmutableSet<IncludeElementExpression> includeElements,
        Dictionary<IncludeElementExpression, IImmutableSet<IncludeElementExpression>> updatesInChildren)
    {
        ImmutableHashSet<IncludeElementExpression>.Builder newElementsBuilder = ImmutableHashSet.CreateBuilder<IncludeElementExpression>();
        newElementsBuilder.UnionWith(includeElements);

        foreach ((IncludeElementExpression existingElement, IImmutableSet<IncludeElementExpression> updatedChildren) in updatesInChildren)
        {
            newElementsBuilder.Remove(existingElement);
            newElementsBuilder.Add(new IncludeElementExpression(existingElement.Relationship, updatedChildren));
        }

        return newElementsBuilder.ToImmutable();
    }

    /// <inheritdoc />
    public QueryLayer ComposeForGetById<TId>([DisallowNull] TId id, ResourceType primaryResourceType, TopFieldSelection fieldSelection)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(primaryResourceType);

        FilterExpression? primaryFilter = GetFilterFromResourceDefinition(primaryResourceType);

        if (fieldSelection == TopFieldSelection.OnlyIdAttribute)
        {
            return new QueryLayer(primaryResourceType)
            {
                Filter = CreateFilterForIds(primaryResourceType, primaryFilter, id),
                Selection = CreateSelectionForOnlyId(primaryResourceType)
            };
        }

        ImmutableArray<ExpressionInScope> constraints = [.. _constraintProviders.SelectMany(provider => provider.GetConstraints())];

        var queryLayer = new QueryLayer(primaryResourceType)
        {
            Filter = CreateFilterForIds(primaryResourceType, primaryFilter, id),
            Selection = fieldSelection == TopFieldSelection.WithAllAttributes ? null : GetSelectionForSparseAttributeSet(primaryResourceType)
        };

        queryLayer.Include = ComposeChildren(queryLayer, constraints);

        return queryLayer;
    }

    /// <inheritdoc />
    public QueryLayer ComposeSecondaryLayerForRelationship(ResourceType secondaryResourceType)
    {
        ArgumentNullException.ThrowIfNull(secondaryResourceType);

        ImmutableArray<ExpressionInScope> constraints = [.. _constraintProviders.SelectMany(provider => provider.GetConstraints())];
        ReadOnlyCollection<QueryExpression> expressionsInTopScope = GetExpressionsInTopScope(constraints);

        var secondaryLayer = new QueryLayer(secondaryResourceType)
        {
            Filter = GetFilter(expressionsInTopScope, secondaryResourceType),
            Sort = GetSort(expressionsInTopScope, secondaryResourceType),
            Pagination = GetTopPagination(expressionsInTopScope, secondaryResourceType),
            Selection = CreateSelectionForOnlyId(secondaryResourceType)
        };

        EnsureDeterministicSort(secondaryLayer);

        return secondaryLayer;
    }

    private static FieldSelection CreateSelectionForOnlyId(ResourceType resourceType)
    {
        var selection = new FieldSelection();
        FieldSelectors selectors = selection.GetOrCreateSelectors(resourceType);

        AttrAttribute idAttribute = GetIdAttribute(resourceType);
        selectors.IncludeAttribute(idAttribute);

        return selection;
    }

    /// <inheritdoc />
    public QueryLayer WrapLayerForSecondaryEndpoint<TId>(QueryLayer secondaryLayer, ResourceType primaryResourceType, [DisallowNull] TId primaryId,
        RelationshipAttribute relationship)
    {
        ArgumentNullException.ThrowIfNull(secondaryLayer);
        ArgumentNullException.ThrowIfNull(primaryResourceType);
        ArgumentNullException.ThrowIfNull(primaryId);
        ArgumentNullException.ThrowIfNull(relationship);

        IncludeExpression? innerInclude = secondaryLayer.Include;
        secondaryLayer.Include = null;

        if (relationship is HasOneAttribute)
        {
            secondaryLayer.Sort = null;
        }

        FieldSelection primarySelection = CreateSelectionForOnlyId(primaryResourceType);
        FieldSelectors primarySelectors = primarySelection.GetOrCreateSelectors(primaryResourceType);
        primarySelectors.IncludeRelationship(relationship, secondaryLayer);

        FilterExpression? primaryFilter = GetFilterFromResourceDefinition(primaryResourceType);

        var primaryLayer = new QueryLayer(primaryResourceType)
        {
            Filter = CreateFilterForIds(primaryResourceType, primaryFilter, primaryId),
            Selection = primarySelection,
            Include = RewriteIncludeForSecondaryEndpoint(innerInclude, relationship)
        };

        if (relationship is HasManyAttribute { DisablePagination: true } && secondaryLayer.Pagination != null)
        {
            // Undo pagination/sort. At the time secondaryLayer was being built, we were not yet aware that it needed to be turned off.
            secondaryLayer.Pagination = null;
            _paginationContext.PageSize = null;

            if (secondaryLayer.Sort is { IsAutoGenerated: true })
            {
                secondaryLayer.Sort = null;
            }
        }

        return primaryLayer;
    }

    private static IncludeExpression RewriteIncludeForSecondaryEndpoint(IncludeExpression? relativeInclude, RelationshipAttribute secondaryRelationship)
    {
        IncludeElementExpression parentElement = relativeInclude != null
            ? new IncludeElementExpression(secondaryRelationship, relativeInclude.Elements)
            : new IncludeElementExpression(secondaryRelationship);

        return new IncludeExpression(ImmutableHashSet.Create(parentElement));
    }

    private static FilterExpression? CreateFilterForIds<TId>(ResourceType resourceType, FilterExpression? existingFilter, params ReadOnlySpan<TId> ids)
    {
        if (ids.Length == 0)
        {
            return existingFilter;
        }

        AttrAttribute idAttribute = GetIdAttribute(resourceType);
        var idChain = new ResourceFieldChainExpression(idAttribute);

        FilterExpression innerFilter;

        if (ids.Length == 1)
        {
            var constant = new LiteralConstantExpression(ids[0]!);
            innerFilter = new ComparisonExpression(ComparisonOperator.Equals, idChain, constant);
        }
        else
        {
            ImmutableHashSet<LiteralConstantExpression>.Builder constantsBuilder = ImmutableHashSet.CreateBuilder<LiteralConstantExpression>();

            foreach (TId id in ids)
            {
                constantsBuilder.Add(new LiteralConstantExpression(id!));
            }

            innerFilter = new AnyExpression(idChain, constantsBuilder.ToImmutable());
        }

        return LogicalExpression.Compose(LogicalOperator.And, innerFilter, existingFilter);
    }

    /// <inheritdoc />
    public QueryLayer ComposeForUpdate<TId>([DisallowNull] TId id, ResourceType primaryResourceType)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(primaryResourceType);

        ImmutableHashSet<IncludeElementExpression> includeElements = _targetedFields.Relationships
            .Select(relationship => new IncludeElementExpression(relationship)).ToImmutableHashSet();

        FilterExpression? primaryFilter = GetFilterFromResourceDefinition(primaryResourceType);

        return new QueryLayer(primaryResourceType)
        {
            Filter = CreateFilterForIds(primaryResourceType, primaryFilter, id),
            Include = IncludeExpression.Create(includeElements)
        };
    }

    /// <inheritdoc />
    public IEnumerable<(QueryLayer, RelationshipAttribute)> ComposeForGetTargetedSecondaryResourceIds(IIdentifiable primaryResource)
    {
        ArgumentNullException.ThrowIfNull(primaryResource);

        foreach (RelationshipAttribute relationship in _targetedFields.Relationships)
        {
            object? rightValue = relationship.GetValue(primaryResource);
            HashSet<IIdentifiable> rightResourceIds = CollectionConverter.Instance.ExtractResources(rightValue).ToHashSet(IdentifiableComparer.Instance);

            if (rightResourceIds.Count > 0)
            {
                QueryLayer queryLayer = ComposeForGetRelationshipRightIds(relationship, rightResourceIds);
                yield return (queryLayer, relationship);
            }
        }
    }

    /// <inheritdoc />
    public QueryLayer ComposeForGetRelationshipRightIds(RelationshipAttribute relationship, ICollection<IIdentifiable> rightResourceIds)
    {
        ArgumentNullException.ThrowIfNull(relationship);
        ArgumentNullException.ThrowIfNull(rightResourceIds);

        object[] typedIds = rightResourceIds.Select(resource => resource.GetTypedId()).ToArray();

        FilterExpression? baseFilter = GetFilterFromResourceDefinition(relationship.RightType);
        FilterExpression? filter = CreateFilterForIds(relationship.RightType, baseFilter, typedIds);

        return new QueryLayer(relationship.RightType)
        {
            Filter = filter,
            Selection = CreateSelectionForOnlyId(relationship.RightType)
        };
    }

    /// <inheritdoc />
    public QueryLayer ComposeForHasMany<TId>(HasManyAttribute hasManyRelationship, [DisallowNull] TId leftId, ICollection<IIdentifiable> rightResourceIds)
    {
        ArgumentNullException.ThrowIfNull(hasManyRelationship);
        ArgumentNullException.ThrowIfNull(leftId);
        ArgumentNullException.ThrowIfNull(rightResourceIds);

        object[] rightTypedIds = rightResourceIds.Select(resource => resource.GetTypedId()).ToArray();

        FilterExpression? leftFilter = CreateFilterForIds(hasManyRelationship.LeftType, null, leftId);
        FilterExpression? rightFilter = CreateFilterForIds(hasManyRelationship.RightType, null, rightTypedIds);

        QueryLayer secondaryLayer = new(hasManyRelationship.RightType)
        {
            Filter = rightFilter,
            Selection = CreateSelectionForOnlyId(hasManyRelationship.RightType)
        };

        FieldSelection primarySelection = CreateSelectionForOnlyId(hasManyRelationship.LeftType);
        FieldSelectors primarySelectors = primarySelection.GetOrCreateSelectors(hasManyRelationship.LeftType);
        primarySelectors.IncludeRelationship(hasManyRelationship, secondaryLayer);

        return new QueryLayer(hasManyRelationship.LeftType)
        {
            Filter = leftFilter,
            Selection = primarySelection,
            Include = new IncludeExpression(ImmutableHashSet.Create(new IncludeElementExpression(hasManyRelationship)))
        };
    }

    protected virtual IImmutableSet<IncludeElementExpression> GetIncludeElements(IImmutableSet<IncludeElementExpression> includeElements,
        ResourceType resourceType)
    {
        ArgumentNullException.ThrowIfNull(includeElements);
        ArgumentNullException.ThrowIfNull(resourceType);

        return _resourceDefinitionAccessor.OnApplyIncludes(resourceType, includeElements);
    }

    private FilterExpression? GetFilterFromResourceDefinition(ResourceType resourceType)
    {
        return GetFilter(Array.Empty<QueryExpression>(), resourceType);
    }

    protected virtual FilterExpression? GetFilter(IReadOnlyCollection<QueryExpression> expressionsInScope, ResourceType resourceType)
    {
        ArgumentNullException.ThrowIfNull(expressionsInScope);
        ArgumentNullException.ThrowIfNull(resourceType);

        FilterExpression[] filters = expressionsInScope.OfType<FilterExpression>().ToArray();
        FilterExpression? filter = LogicalExpression.Compose(LogicalOperator.And, filters);

        return _resourceDefinitionAccessor.OnApplyFilter(resourceType, filter);
    }

    protected virtual SortExpression? GetSort(IReadOnlyCollection<QueryExpression> expressionsInScope, ResourceType resourceType)
    {
        ArgumentNullException.ThrowIfNull(expressionsInScope);
        ArgumentNullException.ThrowIfNull(resourceType);

        SortExpression? sort = expressionsInScope.OfType<SortExpression>().FirstOrDefault();

        return _resourceDefinitionAccessor.OnApplySort(resourceType, sort);
    }

    private SortExpression CreateSortById(ResourceType resourceType)
    {
        AttrAttribute idAttribute = GetIdAttribute(resourceType);
        var idAscendingSort = new SortElementExpression(new ResourceFieldChainExpression(idAttribute), true);
        return new SortExpression(ImmutableArray.Create(idAscendingSort), true);
    }

    protected virtual PaginationExpression GetPagination(IReadOnlyCollection<QueryExpression> expressionsInScope, ResourceType resourceType)
    {
        ArgumentNullException.ThrowIfNull(expressionsInScope);
        ArgumentNullException.ThrowIfNull(resourceType);

        PaginationExpression? pagination = expressionsInScope.OfType<PaginationExpression>().FirstOrDefault();

        pagination = _resourceDefinitionAccessor.OnApplyPagination(resourceType, pagination);

        pagination ??= new PaginationExpression(PageNumber.ValueOne, _options.DefaultPageSize);

        return pagination;
    }

    protected virtual FieldSelection? GetSelectionForSparseAttributeSet(ResourceType resourceType)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        var selection = new FieldSelection();

        HashSet<ResourceType> resourceTypes = resourceType.GetAllConcreteDerivedTypes().ToHashSet();
        resourceTypes.Add(resourceType);

        foreach (ResourceType nextType in resourceTypes)
        {
            SparseFieldSetExpression? fieldSetExpression = _sparseFieldSetEvaluator.GetSparseFieldSetForQuery(nextType);

            if (fieldSetExpression != null)
            {
                FieldSelectors selectors = selection.GetOrCreateSelectors(nextType);
                PopulateSelectors(selectors, fieldSetExpression.Elements);

                AttrAttribute idAttribute = GetIdAttribute(nextType);
                selectors.IncludeAttribute(idAttribute);
            }
        }

        return selection.IsEmpty ? null : selection;
    }

    private static void PopulateSelectors(FieldSelectors selectors, IImmutableDictionary<ResourceFieldAttribute, SparseFieldSetElementExpression> elements)
    {
        foreach ((ResourceFieldAttribute field, SparseFieldSetElementExpression element) in elements)
        {
            if (field is AttrAttribute attr)
            {
                selectors.IncludeAttribute(attr);

                if (element.Children.Count > 0)
                {
                    FieldSelectors childSelectors = selectors.GetOrCreateChildSelectors(attr);
                    PopulateSelectors(childSelectors, element.Children);
                }
            }
        }
    }

    private static AttrAttribute GetIdAttribute(ResourceType resourceType)
    {
        return resourceType.GetAttributeByPropertyName(nameof(Identifiable<>.Id));
    }
}
