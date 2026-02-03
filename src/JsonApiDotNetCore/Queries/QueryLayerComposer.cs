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
    private readonly ISparseFieldSetCache _sparseFieldSetCache;

    public QueryLayerComposer(IEnumerable<IQueryConstraintProvider> constraintProviders, IResourceDefinitionAccessor resourceDefinitionAccessor,
        IJsonApiOptions options, IPaginationContext paginationContext, ITargetedFields targetedFields, IEvaluatedIncludeCache evaluatedIncludeCache,
        ISparseFieldSetCache sparseFieldSetCache)
    {
        ArgumentNullException.ThrowIfNull(constraintProviders);
        ArgumentNullException.ThrowIfNull(resourceDefinitionAccessor);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(paginationContext);
        ArgumentNullException.ThrowIfNull(targetedFields);
        ArgumentNullException.ThrowIfNull(evaluatedIncludeCache);
        ArgumentNullException.ThrowIfNull(sparseFieldSetCache);

        _constraintProviders = constraintProviders as IQueryConstraintProvider[] ?? constraintProviders.ToArray();
        _resourceDefinitionAccessor = resourceDefinitionAccessor;
        _options = options;
        _paginationContext = paginationContext;
        _targetedFields = targetedFields;
        _evaluatedIncludeCache = evaluatedIncludeCache;
        _sparseFieldSetCache = sparseFieldSetCache;
    }

    /// <inheritdoc />
    [Obsolete("This method is no longer used and will be removed in a future version.")]
    public FilterExpression? GetPrimaryFilterFromConstraints(ResourceType primaryResourceType)
    {
        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:wrap_before_first_method_call true

        ReadOnlyCollection<FilterExpression> filtersInTopScope = _constraintProviders
            .SelectMany(provider => provider.GetConstraints())
            .Where(constraint => constraint.Scope == null)
            .Select(constraint => constraint.Expression)
            .OfType<FilterExpression>()
            .ToArray()
            .AsReadOnly();

        // @formatter:wrap_before_first_method_call restore
        // @formatter:wrap_chained_method_calls restore

        return GetFilter(filtersInTopScope, primaryResourceType);
    }

    /// <inheritdoc />
    public FilterExpression? GetSecondaryFilterFromConstraints<TId>([DisallowNull] TId primaryId, HasManyAttribute hasManyRelationship)
    {
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

        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:wrap_before_first_method_call true

        ReadOnlyCollection<FilterExpression> filtersInSecondaryScope = _constraintProviders
            .SelectMany(provider => provider.GetConstraints())
            .Where(constraint => constraint.Scope == null)
            .Select(constraint => constraint.Expression)
            .OfType<FilterExpression>()
            .ToArray()
            .AsReadOnly();

        // @formatter:wrap_before_first_method_call restore
        // @formatter:wrap_chained_method_calls restore

        FilterExpression? primaryFilter = GetFilter(Array.Empty<QueryExpression>(), hasManyRelationship.LeftType);

        if (primaryFilter != null && inverseRelationship is HasOneAttribute)
        {
            // We can't lift the field chains in a primary filter, because there's no way for a custom filter expression to express
            // the scope of its chains. See https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/1671.
            return null;
        }

        FilterExpression? secondaryFilter = GetFilter(filtersInSecondaryScope, hasManyRelationship.RightType);
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
        AttrAttribute idAttribute = GetIdAttribute(relationship.LeftType);
        var idChain = new ResourceFieldChainExpression(ImmutableArray.Create<ResourceFieldAttribute>(idAttribute));
        var idComparison = new ComparisonExpression(ComparisonOperator.Equals, idChain, new LiteralConstantExpression(primaryId));

        FilterExpression filter = LogicalExpression.Compose(LogicalOperator.And, idComparison, primaryFilter)!;
        return new HasExpression(new ResourceFieldChainExpression(inverseRelationship), filter);
    }

    /// <inheritdoc />
    public QueryLayer ComposeFromConstraints(ResourceType requestResourceType)
    {
        ArgumentNullException.ThrowIfNull(requestResourceType);

        ImmutableArray<ExpressionInScope> constraints = [.. _constraintProviders.SelectMany(provider => provider.GetConstraints())];

        QueryLayer topLayer = ComposeTopLayer(constraints, requestResourceType);
        topLayer.Include = ComposeChildren(topLayer, constraints);

        _evaluatedIncludeCache.Set(topLayer.Include);

        return topLayer;
    }

    private QueryLayer ComposeTopLayer(ImmutableArray<ExpressionInScope> constraints, ResourceType resourceType)
    {
        using IDisposable _ = CodeTimingSessionManager.Current.Measure("Top-level query composition");

        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:wrap_before_first_method_call true

        ReadOnlyCollection<QueryExpression> expressionsInTopScope = constraints
            .Where(constraint => constraint.Scope == null)
            .Select(constraint => constraint.Expression)
            .ToArray()
            .AsReadOnly();

        // @formatter:wrap_before_first_method_call restore
        // @formatter:wrap_chained_method_calls restore

        PaginationExpression topPagination = GetPagination(expressionsInTopScope, resourceType);
        _paginationContext.PageSize = topPagination.PageSize;
        _paginationContext.PageNumber = topPagination.PageNumber;

        var topLayer = new QueryLayer(resourceType)
        {
            Filter = GetFilter(expressionsInTopScope, resourceType),
            Sort = GetSort(expressionsInTopScope, resourceType),
            Pagination = topPagination,
            Selection = GetSelectionForSparseAttributeSet(resourceType)
        };

        if (topLayer is { Pagination.PageSize: not null, Sort: null })
        {
            topLayer.Sort = CreateSortById(resourceType);
        }

        return topLayer;
    }

    private IncludeExpression ComposeChildren(QueryLayer topLayer, ImmutableArray<ExpressionInScope> constraints)
    {
        using IDisposable _ = CodeTimingSessionManager.Current.Measure("Nested query composition");

        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:wrap_before_first_method_call true

        IncludeExpression include = constraints
            .Where(constraint => constraint.Scope == null)
            .Select(constraint => constraint.Expression)
            .OfType<IncludeExpression>()
            .FirstOrDefault() ?? IncludeExpression.Empty;

        // @formatter:wrap_before_first_method_call restore
        // @formatter:wrap_chained_method_calls restore

        IImmutableSet<IncludeElementExpression> includeElements =
            ProcessIncludeSet(include.Elements, topLayer, ImmutableArray<RelationshipAttribute>.Empty, constraints);

        return !ReferenceEquals(includeElements, include.Elements)
            ? includeElements.Count > 0 ? new IncludeExpression(includeElements) : IncludeExpression.Empty
            : include;
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
        ArgumentNullException.ThrowIfNull(primaryResourceType);

        AttrAttribute idAttribute = GetIdAttribute(primaryResourceType);

        QueryLayer queryLayer = ComposeFromConstraints(primaryResourceType);
        queryLayer.Sort = null;
        queryLayer.Pagination = null;
        queryLayer.Filter = CreateFilterByIds([id], idAttribute, queryLayer.Filter);

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
        ArgumentNullException.ThrowIfNull(secondaryResourceType);

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
    public QueryLayer WrapLayerForSecondaryEndpoint<TId>(QueryLayer secondaryLayer, ResourceType primaryResourceType, [DisallowNull] TId primaryId,
        RelationshipAttribute relationship)
    {
        ArgumentNullException.ThrowIfNull(secondaryLayer);
        ArgumentNullException.ThrowIfNull(primaryResourceType);
        ArgumentNullException.ThrowIfNull(relationship);

        IncludeExpression? innerInclude = secondaryLayer.Include;
        secondaryLayer.Include = null;

        if (relationship is HasOneAttribute)
        {
            secondaryLayer.Sort = null;
        }

        var primarySelection = new FieldSelection();
        FieldSelectors primarySelectors = primarySelection.GetOrCreateSelectors(primaryResourceType);

        IImmutableSet<AttrAttribute> primaryAttributeSet = _sparseFieldSetCache.GetIdAttributeSetForRelationshipQuery(primaryResourceType);
        primarySelectors.IncludeAttributes(primaryAttributeSet);
        primarySelectors.IncludeRelationship(relationship, secondaryLayer);

        FilterExpression? primaryFilter = GetFilter(Array.Empty<QueryExpression>(), primaryResourceType);
        AttrAttribute primaryIdAttribute = GetIdAttribute(primaryResourceType);

        var primaryLayer = new QueryLayer(primaryResourceType)
        {
            Include = RewriteIncludeForSecondaryEndpoint(innerInclude, relationship),
            Filter = CreateFilterByIds([primaryId], primaryIdAttribute, primaryFilter),
            Selection = primarySelection
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

    private IncludeExpression RewriteIncludeForSecondaryEndpoint(IncludeExpression? relativeInclude, RelationshipAttribute secondaryRelationship)
    {
        IncludeElementExpression parentElement = relativeInclude != null
            ? new IncludeElementExpression(secondaryRelationship, relativeInclude.Elements)
            : new IncludeElementExpression(secondaryRelationship);

        return new IncludeExpression(ImmutableHashSet.Create(parentElement));
    }

    private FilterExpression? CreateFilterByIds<TId>(TId[] ids, AttrAttribute idAttribute, FilterExpression? existingFilter)
    {
        var idChain = new ResourceFieldChainExpression(idAttribute);

        FilterExpression? filter = null;

        if (ids.Length == 1)
        {
            var constant = new LiteralConstantExpression(ids.Single()!);
            filter = new ComparisonExpression(ComparisonOperator.Equals, idChain, constant);
        }
        else if (ids.Length > 1)
        {
            ImmutableHashSet<LiteralConstantExpression> constants = ids.Select(id => new LiteralConstantExpression(id!)).ToImmutableHashSet();
            filter = new AnyExpression(idChain, constants);
        }

        return LogicalExpression.Compose(LogicalOperator.And, filter, existingFilter);
    }

    /// <inheritdoc />
    public QueryLayer ComposeForUpdate<TId>([DisallowNull] TId id, ResourceType primaryResourceType)
    {
        ArgumentNullException.ThrowIfNull(primaryResourceType);

        ImmutableHashSet<IncludeElementExpression> includeElements = _targetedFields.Relationships
            .Select(relationship => new IncludeElementExpression(relationship)).ToImmutableHashSet();

        AttrAttribute primaryIdAttribute = GetIdAttribute(primaryResourceType);

        QueryLayer primaryLayer = ComposeTopLayer(ImmutableArray<ExpressionInScope>.Empty, primaryResourceType);
        primaryLayer.Include = includeElements.Count > 0 ? new IncludeExpression(includeElements) : IncludeExpression.Empty;
        primaryLayer.Sort = null;
        primaryLayer.Pagination = null;
        primaryLayer.Filter = CreateFilterByIds([id], primaryIdAttribute, primaryLayer.Filter);
        primaryLayer.Selection = null;

        return primaryLayer;
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

        AttrAttribute rightIdAttribute = GetIdAttribute(relationship.RightType);

        object[] typedIds = rightResourceIds.Select(resource => resource.GetTypedId()).ToArray();

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
    public QueryLayer ComposeForHasMany<TId>(HasManyAttribute hasManyRelationship, [DisallowNull] TId leftId, ICollection<IIdentifiable> rightResourceIds)
    {
        ArgumentNullException.ThrowIfNull(hasManyRelationship);
        ArgumentNullException.ThrowIfNull(rightResourceIds);

        AttrAttribute leftIdAttribute = GetIdAttribute(hasManyRelationship.LeftType);
        AttrAttribute rightIdAttribute = GetIdAttribute(hasManyRelationship.RightType);
        object[] rightTypedIds = rightResourceIds.Select(resource => resource.GetTypedId()).ToArray();

        FilterExpression? leftFilter = CreateFilterByIds([leftId], leftIdAttribute, null);
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
        ArgumentNullException.ThrowIfNull(includeElements);
        ArgumentNullException.ThrowIfNull(resourceType);

        return _resourceDefinitionAccessor.OnApplyIncludes(resourceType, includeElements);
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

#pragma warning disable AV1130 // Return type in method signature should be an interface to an unchangeable collection
    protected virtual FieldSelection? GetSelectionForSparseAttributeSet(ResourceType resourceType)
#pragma warning restore AV1130 // Return type in method signature should be an interface to an unchangeable collection
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        var selection = new FieldSelection();

        HashSet<ResourceType> resourceTypes = resourceType.GetAllConcreteDerivedTypes().ToHashSet();
        resourceTypes.Add(resourceType);

        foreach (ResourceType nextType in resourceTypes)
        {
            IImmutableSet<ResourceFieldAttribute> fieldSet = _sparseFieldSetCache.GetSparseFieldSetForQuery(nextType);

            if (fieldSet.Count == 0)
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
        return resourceType.GetAttributeByPropertyName(nameof(Identifiable<>.Id));
    }
}
