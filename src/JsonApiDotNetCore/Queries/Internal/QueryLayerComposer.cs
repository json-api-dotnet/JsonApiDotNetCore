using System.Collections.Immutable;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Diagnostics;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Internal;

/// <inheritdoc />
[PublicAPI]
public class QueryLayerComposer : IQueryLayerComposer
{
    private readonly CollectionConverter _collectionConverter = new();
    private readonly IEnumerable<IQueryConstraintProvider> _constraintProviders;
    private readonly IResourceDefinitionAccessor _resourceDefinitionAccessor;
    private readonly IJsonApiOptions _options;
    private readonly IPaginationContext _paginationContext;
    private readonly ITargetedFields _targetedFields;
    private readonly IEvaluatedIncludeCache _evaluatedIncludeCache;
    private readonly ISparseFieldSetCache _sparseFieldSetCache;

    public QueryLayerComposer(IEnumerable<IQueryConstraintProvider> constraintProviders, IResourceDefinitionAccessor resourceDefinitionAccessor,
        IJsonApiOptions options, IPaginationContext paginationContext, ITargetedFields targetedFields, IEvaluatedIncludeCache evaluatedIncludeCache,
        ISparseFieldSetCache sparseFieldSetCache)
    {
        ArgumentGuard.NotNull(constraintProviders);
        ArgumentGuard.NotNull(resourceDefinitionAccessor);
        ArgumentGuard.NotNull(options);
        ArgumentGuard.NotNull(paginationContext);
        ArgumentGuard.NotNull(targetedFields);
        ArgumentGuard.NotNull(evaluatedIncludeCache);
        ArgumentGuard.NotNull(sparseFieldSetCache);

        _constraintProviders = constraintProviders;
        _resourceDefinitionAccessor = resourceDefinitionAccessor;
        _options = options;
        _paginationContext = paginationContext;
        _targetedFields = targetedFields;
        _evaluatedIncludeCache = evaluatedIncludeCache;
        _sparseFieldSetCache = sparseFieldSetCache;
    }

    /// <inheritdoc />
    public FilterExpression? GetPrimaryFilterFromConstraints(ResourceType primaryResourceType)
    {
        ExpressionInScope[] constraints = _constraintProviders.SelectMany(provider => provider.GetConstraints()).ToArray();

        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:keep_existing_linebreaks true

        FilterExpression[] filtersInTopScope = constraints
            .Where(constraint => constraint.Scope == null)
            .Select(constraint => constraint.Expression)
            .OfType<FilterExpression>()
            .ToArray();

        // @formatter:keep_existing_linebreaks restore
        // @formatter:wrap_chained_method_calls restore

        return GetFilter(filtersInTopScope, primaryResourceType);
    }

    /// <inheritdoc />
    public FilterExpression? GetSecondaryFilterFromConstraints<TId>(TId primaryId, HasManyAttribute hasManyRelationship)
    {
        ArgumentGuard.NotNull(hasManyRelationship);

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

        ExpressionInScope[] constraints = _constraintProviders.SelectMany(provider => provider.GetConstraints()).ToArray();

        var secondaryScope = new ResourceFieldChainExpression(hasManyRelationship);

        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:keep_existing_linebreaks true

        FilterExpression[] filtersInSecondaryScope = constraints
            .Where(constraint => secondaryScope.Equals(constraint.Scope))
            .Select(constraint => constraint.Expression)
            .OfType<FilterExpression>()
            .ToArray();

        // @formatter:keep_existing_linebreaks restore
        // @formatter:wrap_chained_method_calls restore

        FilterExpression? primaryFilter = GetFilter(Array.Empty<QueryExpression>(), hasManyRelationship.LeftType);
        FilterExpression? secondaryFilter = GetFilter(filtersInSecondaryScope, hasManyRelationship.RightType);

        FilterExpression inverseFilter = GetInverseRelationshipFilter(primaryId, hasManyRelationship, inverseRelationship);

        return LogicalExpression.Compose(LogicalOperator.And, inverseFilter, primaryFilter, secondaryFilter);
    }

    private static FilterExpression GetInverseRelationshipFilter<TId>(TId primaryId, HasManyAttribute relationship, RelationshipAttribute inverseRelationship)
    {
        return inverseRelationship is HasManyAttribute hasManyInverseRelationship
            ? GetInverseHasManyRelationshipFilter(primaryId, relationship, hasManyInverseRelationship)
            : GetInverseHasOneRelationshipFilter(primaryId, relationship, (HasOneAttribute)inverseRelationship);
    }

    private static FilterExpression GetInverseHasOneRelationshipFilter<TId>(TId primaryId, HasManyAttribute relationship, HasOneAttribute inverseRelationship)
    {
        AttrAttribute idAttribute = GetIdAttribute(relationship.LeftType);
        var idChain = new ResourceFieldChainExpression(ImmutableArray.Create<ResourceFieldAttribute>(inverseRelationship, idAttribute));

        return new ComparisonExpression(ComparisonOperator.Equals, idChain, new LiteralConstantExpression(primaryId!.ToString()!));
    }

    private static FilterExpression GetInverseHasManyRelationshipFilter<TId>(TId primaryId, HasManyAttribute relationship, HasManyAttribute inverseRelationship)
    {
        AttrAttribute idAttribute = GetIdAttribute(relationship.LeftType);
        var idChain = new ResourceFieldChainExpression(ImmutableArray.Create<ResourceFieldAttribute>(idAttribute));
        var idComparison = new ComparisonExpression(ComparisonOperator.Equals, idChain, new LiteralConstantExpression(primaryId!.ToString()!));

        return new HasExpression(new ResourceFieldChainExpression(inverseRelationship), idComparison);
    }

    /// <inheritdoc />
    public QueryLayer ComposeFromConstraints(ResourceType requestResourceType)
    {
        ArgumentGuard.NotNull(requestResourceType);

        ExpressionInScope[] constraints = _constraintProviders.SelectMany(provider => provider.GetConstraints()).ToArray();

        QueryLayer topLayer = ComposeTopLayer(constraints, requestResourceType);
        topLayer.Include = ComposeChildren(topLayer, constraints);

        _evaluatedIncludeCache.Set(topLayer.Include);

        return topLayer;
    }

    private QueryLayer ComposeTopLayer(IEnumerable<ExpressionInScope> constraints, ResourceType resourceType)
    {
        using IDisposable _ = CodeTimingSessionManager.Current.Measure("Top-level query composition");

        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:keep_existing_linebreaks true

        QueryExpression[] expressionsInTopScope = constraints
            .Where(constraint => constraint.Scope == null)
            .Select(constraint => constraint.Expression)
            .ToArray();

        // @formatter:keep_existing_linebreaks restore
        // @formatter:wrap_chained_method_calls restore

        PaginationExpression topPagination = GetPagination(expressionsInTopScope, resourceType);
        _paginationContext.PageSize = topPagination.PageSize;
        _paginationContext.PageNumber = topPagination.PageNumber;

        return new QueryLayer(resourceType)
        {
            Filter = GetFilter(expressionsInTopScope, resourceType),
            Sort = GetSort(expressionsInTopScope, resourceType),
            Pagination = topPagination,
            Selection = GetSelectionForSparseAttributeSet(resourceType)
        };
    }

    private IncludeExpression ComposeChildren(QueryLayer topLayer, ICollection<ExpressionInScope> constraints)
    {
        using IDisposable _ = CodeTimingSessionManager.Current.Measure("Nested query composition");

        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:keep_existing_linebreaks true

        IncludeExpression include = constraints
            .Where(constraint => constraint.Scope == null)
            .Select(constraint => constraint.Expression)
            .OfType<IncludeExpression>()
            .FirstOrDefault() ?? IncludeExpression.Empty;

        // @formatter:keep_existing_linebreaks restore
        // @formatter:wrap_chained_method_calls restore

        IImmutableSet<IncludeElementExpression> includeElements = ProcessIncludeSet(include.Elements, topLayer, new List<RelationshipAttribute>(), constraints);

        return !ReferenceEquals(includeElements, include.Elements)
            ? includeElements.Any() ? new IncludeExpression(includeElements) : IncludeExpression.Empty
            : include;
    }

    private IImmutableSet<IncludeElementExpression> ProcessIncludeSet(IImmutableSet<IncludeElementExpression> includeElements, QueryLayer parentLayer,
        ICollection<RelationshipAttribute> parentRelationshipChain, ICollection<ExpressionInScope> constraints)
    {
        IImmutableSet<IncludeElementExpression> includeElementsEvaluated = GetIncludeElements(includeElements, parentLayer.ResourceType);

        var updatesInChildren = new Dictionary<IncludeElementExpression, IImmutableSet<IncludeElementExpression>>();

        foreach (IncludeElementExpression includeElement in includeElementsEvaluated)
        {
            parentLayer.Selection ??= new FieldSelection();
            FieldSelectors selectors = parentLayer.Selection.GetOrCreateSelectors(parentLayer.ResourceType);

            if (!selectors.ContainsField(includeElement.Relationship))
            {
                var relationshipChain = new List<RelationshipAttribute>(parentRelationshipChain)
                {
                    includeElement.Relationship
                };

                // @formatter:wrap_chained_method_calls chop_always
                // @formatter:keep_existing_linebreaks true

                QueryExpression[] expressionsInCurrentScope = constraints
                    .Where(constraint =>
                        constraint.Scope != null && constraint.Scope.Fields.SequenceEqual(relationshipChain))
                    .Select(constraint => constraint.Expression)
                    .ToArray();

                // @formatter:keep_existing_linebreaks restore
                // @formatter:wrap_chained_method_calls restore

                ResourceType resourceType = includeElement.Relationship.RightType;
                bool isToManyRelationship = includeElement.Relationship is HasManyAttribute;

                var child = new QueryLayer(resourceType)
                {
                    Filter = isToManyRelationship ? GetFilter(expressionsInCurrentScope, resourceType) : null,
                    Sort = isToManyRelationship ? GetSort(expressionsInCurrentScope, resourceType) : null,
                    Pagination = isToManyRelationship ? GetPagination(expressionsInCurrentScope, resourceType) : null,
                    Selection = GetSelectionForSparseAttributeSet(resourceType)
                };

                selectors.IncludeRelationship(includeElement.Relationship, child);

                IImmutableSet<IncludeElementExpression> updatedChildren = ProcessIncludeSet(includeElement.Children, child, relationshipChain, constraints);

                if (!ReferenceEquals(includeElement.Children, updatedChildren))
                {
                    updatesInChildren.Add(includeElement, updatedChildren);
                }
            }
        }

        return !updatesInChildren.Any() ? includeElementsEvaluated : ApplyIncludeElementUpdates(includeElementsEvaluated, updatesInChildren);
    }

    private static IImmutableSet<IncludeElementExpression> ApplyIncludeElementUpdates(IImmutableSet<IncludeElementExpression> includeElements,
        IDictionary<IncludeElementExpression, IImmutableSet<IncludeElementExpression>> updatesInChildren)
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
    public QueryLayer ComposeForGetById<TId>(TId id, ResourceType primaryResourceType, TopFieldSelection fieldSelection)
    {
        ArgumentGuard.NotNull(primaryResourceType);

        AttrAttribute idAttribute = GetIdAttribute(primaryResourceType);

        QueryLayer queryLayer = ComposeFromConstraints(primaryResourceType);
        queryLayer.Sort = null;
        queryLayer.Pagination = null;
        queryLayer.Filter = CreateFilterByIds(id.AsArray(), idAttribute, queryLayer.Filter);

        if (fieldSelection == TopFieldSelection.OnlyIdAttribute)
        {
            queryLayer.Selection = new FieldSelection();
            FieldSelectors selectors = queryLayer.Selection.GetOrCreateSelectors(primaryResourceType);
            selectors.IncludeAttribute(idAttribute);
        }
        else if (fieldSelection == TopFieldSelection.WithAllAttributes && queryLayer.Selection != null)
        {
            // Discard any top-level ?fields[]= or attribute exclusions from resource definition, because we need the full database row.
            FieldSelectors selectors = queryLayer.Selection.GetOrCreateSelectors(primaryResourceType);
            selectors.RemoveAttributes();
        }

        return queryLayer;
    }

    /// <inheritdoc />
    public QueryLayer ComposeSecondaryLayerForRelationship(ResourceType secondaryResourceType)
    {
        ArgumentGuard.NotNull(secondaryResourceType);

        QueryLayer secondaryLayer = ComposeFromConstraints(secondaryResourceType);
        secondaryLayer.Selection = GetSelectionForRelationship(secondaryResourceType);
        secondaryLayer.Include = null;

        return secondaryLayer;
    }

    private FieldSelection GetSelectionForRelationship(ResourceType secondaryResourceType)
    {
        var selection = new FieldSelection();
        FieldSelectors selectors = selection.GetOrCreateSelectors(secondaryResourceType);

        IImmutableSet<AttrAttribute> secondaryAttributeSet = _sparseFieldSetCache.GetIdAttributeSetForRelationshipQuery(secondaryResourceType);
        selectors.IncludeAttributes(secondaryAttributeSet);

        return selection;
    }

    /// <inheritdoc />
    public QueryLayer WrapLayerForSecondaryEndpoint<TId>(QueryLayer secondaryLayer, ResourceType primaryResourceType, TId primaryId,
        RelationshipAttribute relationship)
    {
        ArgumentGuard.NotNull(secondaryLayer);
        ArgumentGuard.NotNull(primaryResourceType);
        ArgumentGuard.NotNull(relationship);

        IncludeExpression? innerInclude = secondaryLayer.Include;
        secondaryLayer.Include = null;

        var primarySelection = new FieldSelection();
        FieldSelectors primarySelectors = primarySelection.GetOrCreateSelectors(primaryResourceType);

        IImmutableSet<AttrAttribute> primaryAttributeSet = _sparseFieldSetCache.GetIdAttributeSetForRelationshipQuery(primaryResourceType);
        primarySelectors.IncludeAttributes(primaryAttributeSet);
        primarySelectors.IncludeRelationship(relationship, secondaryLayer);

        FilterExpression? primaryFilter = GetFilter(Array.Empty<QueryExpression>(), primaryResourceType);
        AttrAttribute primaryIdAttribute = GetIdAttribute(primaryResourceType);

        return new QueryLayer(primaryResourceType)
        {
            Include = RewriteIncludeForSecondaryEndpoint(innerInclude, relationship),
            Filter = CreateFilterByIds(primaryId.AsArray(), primaryIdAttribute, primaryFilter),
            Selection = primarySelection
        };
    }

    private IncludeExpression RewriteIncludeForSecondaryEndpoint(IncludeExpression? relativeInclude, RelationshipAttribute secondaryRelationship)
    {
        IncludeElementExpression parentElement = relativeInclude != null
            ? new IncludeElementExpression(secondaryRelationship, relativeInclude.Elements)
            : new IncludeElementExpression(secondaryRelationship);

        return new IncludeExpression(ImmutableHashSet.Create(parentElement));
    }

    private FilterExpression? CreateFilterByIds<TId>(IReadOnlyCollection<TId> ids, AttrAttribute idAttribute, FilterExpression? existingFilter)
    {
        var idChain = new ResourceFieldChainExpression(idAttribute);

        FilterExpression? filter = null;

        if (ids.Count == 1)
        {
            var constant = new LiteralConstantExpression(ids.Single()!.ToString()!);
            filter = new ComparisonExpression(ComparisonOperator.Equals, idChain, constant);
        }
        else if (ids.Count > 1)
        {
            ImmutableHashSet<LiteralConstantExpression> constants = ids.Select(id => new LiteralConstantExpression(id!.ToString()!)).ToImmutableHashSet();
            filter = new AnyExpression(idChain, constants);
        }

        return LogicalExpression.Compose(LogicalOperator.And, filter, existingFilter);
    }

    /// <inheritdoc />
    public QueryLayer ComposeForUpdate<TId>(TId id, ResourceType primaryResourceType)
    {
        ArgumentGuard.NotNull(primaryResourceType);

        IImmutableSet<IncludeElementExpression> includeElements = _targetedFields.Relationships
            .Select(relationship => new IncludeElementExpression(relationship)).ToImmutableHashSet();

        AttrAttribute primaryIdAttribute = GetIdAttribute(primaryResourceType);

        QueryLayer primaryLayer = ComposeTopLayer(Array.Empty<ExpressionInScope>(), primaryResourceType);
        primaryLayer.Include = includeElements.Any() ? new IncludeExpression(includeElements) : IncludeExpression.Empty;
        primaryLayer.Sort = null;
        primaryLayer.Pagination = null;
        primaryLayer.Filter = CreateFilterByIds(id.AsArray(), primaryIdAttribute, primaryLayer.Filter);
        primaryLayer.Selection = null;

        return primaryLayer;
    }

    /// <inheritdoc />
    public IEnumerable<(QueryLayer, RelationshipAttribute)> ComposeForGetTargetedSecondaryResourceIds(IIdentifiable primaryResource)
    {
        ArgumentGuard.NotNull(primaryResource);

        foreach (RelationshipAttribute relationship in _targetedFields.Relationships)
        {
            object? rightValue = relationship.GetValue(primaryResource);
            HashSet<IIdentifiable> rightResourceIds = _collectionConverter.ExtractResources(rightValue).ToHashSet(IdentifiableComparer.Instance);

            if (rightResourceIds.Any())
            {
                QueryLayer queryLayer = ComposeForGetRelationshipRightIds(relationship, rightResourceIds);
                yield return (queryLayer, relationship);
            }
        }
    }

    /// <inheritdoc />
    public QueryLayer ComposeForGetRelationshipRightIds(RelationshipAttribute relationship, ICollection<IIdentifiable> rightResourceIds)
    {
        ArgumentGuard.NotNull(relationship);
        ArgumentGuard.NotNull(rightResourceIds);

        AttrAttribute rightIdAttribute = GetIdAttribute(relationship.RightType);

        HashSet<object> typedIds = rightResourceIds.Select(resource => resource.GetTypedId()).ToHashSet();

        FilterExpression? baseFilter = GetFilter(Array.Empty<QueryExpression>(), relationship.RightType);
        FilterExpression? filter = CreateFilterByIds(typedIds, rightIdAttribute, baseFilter);

        var selection = new FieldSelection();
        FieldSelectors selectors = selection.GetOrCreateSelectors(relationship.RightType);
        selectors.IncludeAttribute(rightIdAttribute);

        return new QueryLayer(relationship.RightType)
        {
            Include = IncludeExpression.Empty,
            Filter = filter,
            Selection = selection
        };
    }

    /// <inheritdoc />
    public QueryLayer ComposeForHasMany<TId>(HasManyAttribute hasManyRelationship, TId leftId, ICollection<IIdentifiable> rightResourceIds)
    {
        ArgumentGuard.NotNull(hasManyRelationship);
        ArgumentGuard.NotNull(rightResourceIds);

        AttrAttribute leftIdAttribute = GetIdAttribute(hasManyRelationship.LeftType);
        AttrAttribute rightIdAttribute = GetIdAttribute(hasManyRelationship.RightType);
        HashSet<object> rightTypedIds = rightResourceIds.Select(resource => resource.GetTypedId()).ToHashSet();

        FilterExpression? leftFilter = CreateFilterByIds(leftId.AsArray(), leftIdAttribute, null);
        FilterExpression? rightFilter = CreateFilterByIds(rightTypedIds, rightIdAttribute, null);

        var secondarySelection = new FieldSelection();
        FieldSelectors secondarySelectors = secondarySelection.GetOrCreateSelectors(hasManyRelationship.RightType);
        secondarySelectors.IncludeAttribute(rightIdAttribute);

        QueryLayer secondaryLayer = new(hasManyRelationship.RightType)
        {
            Filter = rightFilter,
            Selection = secondarySelection
        };

        var primarySelection = new FieldSelection();
        FieldSelectors primarySelectors = primarySelection.GetOrCreateSelectors(hasManyRelationship.LeftType);
        primarySelectors.IncludeRelationship(hasManyRelationship, secondaryLayer);
        primarySelectors.IncludeAttribute(leftIdAttribute);

        return new QueryLayer(hasManyRelationship.LeftType)
        {
            Include = new IncludeExpression(ImmutableHashSet.Create(new IncludeElementExpression(hasManyRelationship))),
            Filter = leftFilter,
            Selection = primarySelection
        };
    }

    protected virtual IImmutableSet<IncludeElementExpression> GetIncludeElements(IImmutableSet<IncludeElementExpression> includeElements,
        ResourceType resourceType)
    {
        ArgumentGuard.NotNull(resourceType);

        return _resourceDefinitionAccessor.OnApplyIncludes(resourceType, includeElements);
    }

    protected virtual FilterExpression? GetFilter(IReadOnlyCollection<QueryExpression> expressionsInScope, ResourceType resourceType)
    {
        ArgumentGuard.NotNull(expressionsInScope);
        ArgumentGuard.NotNull(resourceType);

        FilterExpression[] filters = expressionsInScope.OfType<FilterExpression>().ToArray();
        FilterExpression? filter = LogicalExpression.Compose(LogicalOperator.And, filters);

        return _resourceDefinitionAccessor.OnApplyFilter(resourceType, filter);
    }

    protected virtual SortExpression GetSort(IReadOnlyCollection<QueryExpression> expressionsInScope, ResourceType resourceType)
    {
        ArgumentGuard.NotNull(expressionsInScope);
        ArgumentGuard.NotNull(resourceType);

        SortExpression? sort = expressionsInScope.OfType<SortExpression>().FirstOrDefault();

        sort = _resourceDefinitionAccessor.OnApplySort(resourceType, sort);

        if (sort == null)
        {
            AttrAttribute idAttribute = GetIdAttribute(resourceType);
            var idAscendingSort = new SortElementExpression(new ResourceFieldChainExpression(idAttribute), true);
            sort = new SortExpression(ImmutableArray.Create(idAscendingSort));
        }

        return sort;
    }

    protected virtual PaginationExpression GetPagination(IReadOnlyCollection<QueryExpression> expressionsInScope, ResourceType resourceType)
    {
        ArgumentGuard.NotNull(expressionsInScope);
        ArgumentGuard.NotNull(resourceType);

        PaginationExpression? pagination = expressionsInScope.OfType<PaginationExpression>().FirstOrDefault();

        pagination = _resourceDefinitionAccessor.OnApplyPagination(resourceType, pagination);

        pagination ??= new PaginationExpression(PageNumber.ValueOne, _options.DefaultPageSize);

        return pagination;
    }

#pragma warning disable AV1130 // Return type in method signature should be an interface to an unchangeable collection
    protected virtual FieldSelection? GetSelectionForSparseAttributeSet(ResourceType resourceType)
#pragma warning restore AV1130 // Return type in method signature should be an interface to an unchangeable collection
    {
        ArgumentGuard.NotNull(resourceType);

        var selection = new FieldSelection();

        HashSet<ResourceType> resourceTypes = resourceType.GetAllConcreteDerivedTypes().ToHashSet();
        resourceTypes.Add(resourceType);

        foreach (ResourceType nextType in resourceTypes)
        {
            IImmutableSet<ResourceFieldAttribute> fieldSet = _sparseFieldSetCache.GetSparseFieldSetForQuery(nextType);

            if (!fieldSet.Any())
            {
                continue;
            }

            HashSet<AttrAttribute> attributeSet = fieldSet.OfType<AttrAttribute>().ToHashSet();

            FieldSelectors selectors = selection.GetOrCreateSelectors(nextType);
            selectors.IncludeAttributes(attributeSet);

            AttrAttribute idAttribute = GetIdAttribute(nextType);
            selectors.IncludeAttribute(idAttribute);
        }

        return selection.IsEmpty ? null : selection;
    }

    private static AttrAttribute GetIdAttribute(ResourceType resourceType)
    {
        return resourceType.GetAttributeByPropertyName(nameof(Identifiable<object>.Id));
    }
}
