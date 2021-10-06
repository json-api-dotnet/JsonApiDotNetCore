using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Diagnostics;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Internal
{
    /// <inheritdoc />
    [PublicAPI]
    public class QueryLayerComposer : IQueryLayerComposer
    {
        private readonly CollectionConverter _collectionConverter = new();
        private readonly IEnumerable<IQueryConstraintProvider> _constraintProviders;
        private readonly IResourceGraph _resourceGraph;
        private readonly IResourceDefinitionAccessor _resourceDefinitionAccessor;
        private readonly IJsonApiOptions _options;
        private readonly IPaginationContext _paginationContext;
        private readonly ITargetedFields _targetedFields;
        private readonly IEvaluatedIncludeCache _evaluatedIncludeCache;
        private readonly ISparseFieldSetCache _sparseFieldSetCache;

        public QueryLayerComposer(IEnumerable<IQueryConstraintProvider> constraintProviders, IResourceGraph resourceGraph,
            IResourceDefinitionAccessor resourceDefinitionAccessor, IJsonApiOptions options, IPaginationContext paginationContext,
            ITargetedFields targetedFields, IEvaluatedIncludeCache evaluatedIncludeCache, ISparseFieldSetCache sparseFieldSetCache)
        {
            ArgumentGuard.NotNull(constraintProviders, nameof(constraintProviders));
            ArgumentGuard.NotNull(resourceGraph, nameof(resourceGraph));
            ArgumentGuard.NotNull(resourceDefinitionAccessor, nameof(resourceDefinitionAccessor));
            ArgumentGuard.NotNull(options, nameof(options));
            ArgumentGuard.NotNull(paginationContext, nameof(paginationContext));
            ArgumentGuard.NotNull(targetedFields, nameof(targetedFields));
            ArgumentGuard.NotNull(evaluatedIncludeCache, nameof(evaluatedIncludeCache));
            ArgumentGuard.NotNull(sparseFieldSetCache, nameof(sparseFieldSetCache));

            _constraintProviders = constraintProviders;
            _resourceGraph = resourceGraph;
            _resourceDefinitionAccessor = resourceDefinitionAccessor;
            _options = options;
            _paginationContext = paginationContext;
            _targetedFields = targetedFields;
            _evaluatedIncludeCache = evaluatedIncludeCache;
            _sparseFieldSetCache = sparseFieldSetCache;
        }

        /// <inheritdoc />
        public FilterExpression GetTopFilterFromConstraints(ResourceContext resourceContext)
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

            return GetFilter(filtersInTopScope, resourceContext);
        }

        /// <inheritdoc />
        public QueryLayer ComposeFromConstraints(ResourceContext requestResource)
        {
            ArgumentGuard.NotNull(requestResource, nameof(requestResource));

            ExpressionInScope[] constraints = _constraintProviders.SelectMany(provider => provider.GetConstraints()).ToArray();

            QueryLayer topLayer = ComposeTopLayer(constraints, requestResource);
            topLayer.Include = ComposeChildren(topLayer, constraints);

            _evaluatedIncludeCache.Set(topLayer.Include);

            return topLayer;
        }

        private QueryLayer ComposeTopLayer(IEnumerable<ExpressionInScope> constraints, ResourceContext resourceContext)
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

            PaginationExpression topPagination = GetPagination(expressionsInTopScope, resourceContext);

            if (topPagination != null)
            {
                _paginationContext.PageSize = topPagination.PageSize;
                _paginationContext.PageNumber = topPagination.PageNumber;
            }

            return new QueryLayer(resourceContext)
            {
                Filter = GetFilter(expressionsInTopScope, resourceContext),
                Sort = GetSort(expressionsInTopScope, resourceContext),
                Pagination = ((JsonApiOptions)_options).DisableTopPagination ? null : topPagination,
                Projection = GetProjectionForSparseAttributeSet(resourceContext)
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

            IImmutableSet<IncludeElementExpression> includeElements =
                ProcessIncludeSet(include.Elements, topLayer, new List<RelationshipAttribute>(), constraints);

            return !ReferenceEquals(includeElements, include.Elements)
                ? includeElements.Any() ? new IncludeExpression(includeElements) : IncludeExpression.Empty
                : include;
        }

        private IImmutableSet<IncludeElementExpression> ProcessIncludeSet(IImmutableSet<IncludeElementExpression> includeElements, QueryLayer parentLayer,
            ICollection<RelationshipAttribute> parentRelationshipChain, ICollection<ExpressionInScope> constraints)
        {
            IImmutableSet<IncludeElementExpression> includeElementsEvaluated =
                GetIncludeElements(includeElements, parentLayer.ResourceContext) ?? ImmutableHashSet<IncludeElementExpression>.Empty;

            var updatesInChildren = new Dictionary<IncludeElementExpression, IImmutableSet<IncludeElementExpression>>();

            foreach (IncludeElementExpression includeElement in includeElementsEvaluated)
            {
                parentLayer.Projection ??= new Dictionary<ResourceFieldAttribute, QueryLayer>();

                if (!parentLayer.Projection.ContainsKey(includeElement.Relationship))
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

                    ResourceContext resourceContext = _resourceGraph.GetResourceContext(includeElement.Relationship.RightType);
                    bool isToManyRelationship = includeElement.Relationship is HasManyAttribute;

                    var child = new QueryLayer(resourceContext)
                    {
                        Filter = isToManyRelationship ? GetFilter(expressionsInCurrentScope, resourceContext) : null,
                        Sort = isToManyRelationship ? GetSort(expressionsInCurrentScope, resourceContext) : null,
                        Pagination = isToManyRelationship
                            ? ((JsonApiOptions)_options).DisableChildrenPagination ? null : GetPagination(expressionsInCurrentScope, resourceContext)
                            : null,
                        Projection = GetProjectionForSparseAttributeSet(resourceContext)
                    };

                    parentLayer.Projection.Add(includeElement.Relationship, child);

                    IImmutableSet<IncludeElementExpression> updatedChildren =
                        ProcessIncludeSet(includeElement.Children, child, relationshipChain, constraints);

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
            newElementsBuilder.AddRange(includeElements);

            foreach ((IncludeElementExpression existingElement, IImmutableSet<IncludeElementExpression> updatedChildren) in updatesInChildren)
            {
                newElementsBuilder.Remove(existingElement);
                newElementsBuilder.Add(new IncludeElementExpression(existingElement.Relationship, updatedChildren));
            }

            return newElementsBuilder.ToImmutable();
        }

        /// <inheritdoc />
        public QueryLayer ComposeForGetById<TId>(TId id, ResourceContext resourceContext, TopFieldSelection fieldSelection)
        {
            ArgumentGuard.NotNull(resourceContext, nameof(resourceContext));

            AttrAttribute idAttribute = GetIdAttribute(resourceContext);

            QueryLayer queryLayer = ComposeFromConstraints(resourceContext);
            queryLayer.Sort = null;
            queryLayer.Pagination = null;
            queryLayer.Filter = CreateFilterByIds(id.AsArray(), idAttribute, queryLayer.Filter);

            if (fieldSelection == TopFieldSelection.OnlyIdAttribute)
            {
                queryLayer.Projection = new Dictionary<ResourceFieldAttribute, QueryLayer>
                {
                    [idAttribute] = null
                };
            }
            else if (fieldSelection == TopFieldSelection.WithAllAttributes && queryLayer.Projection != null)
            {
                // Discard any top-level ?fields[]= or attribute exclusions from resource definition, because we need the full database row.
                while (queryLayer.Projection.Any(pair => pair.Key is AttrAttribute))
                {
                    queryLayer.Projection.Remove(queryLayer.Projection.First(pair => pair.Key is AttrAttribute));
                }
            }

            return queryLayer;
        }

        /// <inheritdoc />
        public QueryLayer ComposeSecondaryLayerForRelationship(ResourceContext secondaryResourceContext)
        {
            ArgumentGuard.NotNull(secondaryResourceContext, nameof(secondaryResourceContext));

            QueryLayer secondaryLayer = ComposeFromConstraints(secondaryResourceContext);
            secondaryLayer.Projection = GetProjectionForRelationship(secondaryResourceContext);
            secondaryLayer.Include = null;

            return secondaryLayer;
        }

        private IDictionary<ResourceFieldAttribute, QueryLayer> GetProjectionForRelationship(ResourceContext secondaryResourceContext)
        {
            IImmutableSet<AttrAttribute> secondaryAttributeSet = _sparseFieldSetCache.GetIdAttributeSetForRelationshipQuery(secondaryResourceContext);

            return secondaryAttributeSet.ToDictionary(key => (ResourceFieldAttribute)key, _ => (QueryLayer)null);
        }

        /// <inheritdoc />
        public QueryLayer WrapLayerForSecondaryEndpoint<TId>(QueryLayer secondaryLayer, ResourceContext primaryResourceContext, TId primaryId,
            RelationshipAttribute secondaryRelationship)
        {
            ArgumentGuard.NotNull(secondaryLayer, nameof(secondaryLayer));
            ArgumentGuard.NotNull(primaryResourceContext, nameof(primaryResourceContext));
            ArgumentGuard.NotNull(secondaryRelationship, nameof(secondaryRelationship));

            IncludeExpression innerInclude = secondaryLayer.Include;
            secondaryLayer.Include = null;

            IImmutableSet<AttrAttribute> primaryAttributeSet = _sparseFieldSetCache.GetIdAttributeSetForRelationshipQuery(primaryResourceContext);

            Dictionary<ResourceFieldAttribute, QueryLayer> primaryProjection =
                primaryAttributeSet.ToDictionary(key => (ResourceFieldAttribute)key, _ => (QueryLayer)null);

            primaryProjection[secondaryRelationship] = secondaryLayer;

            FilterExpression primaryFilter = GetFilter(Array.Empty<QueryExpression>(), primaryResourceContext);
            AttrAttribute primaryIdAttribute = GetIdAttribute(primaryResourceContext);

            return new QueryLayer(primaryResourceContext)
            {
                Include = RewriteIncludeForSecondaryEndpoint(innerInclude, secondaryRelationship),
                Filter = CreateFilterByIds(primaryId.AsArray(), primaryIdAttribute, primaryFilter),
                Projection = primaryProjection
            };
        }

        private IncludeExpression RewriteIncludeForSecondaryEndpoint(IncludeExpression relativeInclude, RelationshipAttribute secondaryRelationship)
        {
            IncludeElementExpression parentElement = relativeInclude != null
                ? new IncludeElementExpression(secondaryRelationship, relativeInclude.Elements)
                : new IncludeElementExpression(secondaryRelationship);

            return new IncludeExpression(ImmutableHashSet.Create(parentElement));
        }

        private FilterExpression CreateFilterByIds<TId>(IReadOnlyCollection<TId> ids, AttrAttribute idAttribute, FilterExpression existingFilter)
        {
            var idChain = new ResourceFieldChainExpression(idAttribute);

            FilterExpression filter = null;

            if (ids.Count == 1)
            {
                var constant = new LiteralConstantExpression(ids.Single().ToString());
                filter = new ComparisonExpression(ComparisonOperator.Equals, idChain, constant);
            }
            else if (ids.Count > 1)
            {
                ImmutableHashSet<LiteralConstantExpression> constants = ids.Select(id => new LiteralConstantExpression(id.ToString())).ToImmutableHashSet();
                filter = new AnyExpression(idChain, constants);
            }

            return filter == null ? existingFilter : existingFilter == null ? filter : new LogicalExpression(LogicalOperator.And, filter, existingFilter);
        }

        /// <inheritdoc />
        public QueryLayer ComposeForUpdate<TId>(TId id, ResourceContext primaryResource)
        {
            ArgumentGuard.NotNull(primaryResource, nameof(primaryResource));

            IImmutableSet<IncludeElementExpression> includeElements = _targetedFields.Relationships
                .Select(relationship => new IncludeElementExpression(relationship)).ToImmutableHashSet();

            AttrAttribute primaryIdAttribute = GetIdAttribute(primaryResource);

            QueryLayer primaryLayer = ComposeTopLayer(Array.Empty<ExpressionInScope>(), primaryResource);
            primaryLayer.Include = includeElements.Any() ? new IncludeExpression(includeElements) : IncludeExpression.Empty;
            primaryLayer.Sort = null;
            primaryLayer.Pagination = null;
            primaryLayer.Filter = CreateFilterByIds(id.AsArray(), primaryIdAttribute, primaryLayer.Filter);
            primaryLayer.Projection = null;

            return primaryLayer;
        }

        /// <inheritdoc />
        public IEnumerable<(QueryLayer, RelationshipAttribute)> ComposeForGetTargetedSecondaryResourceIds(IIdentifiable primaryResource)
        {
            ArgumentGuard.NotNull(primaryResource, nameof(primaryResource));

            foreach (RelationshipAttribute relationship in _targetedFields.Relationships)
            {
                object rightValue = relationship.GetValue(primaryResource);
                ICollection<IIdentifiable> rightResourceIds = _collectionConverter.ExtractResources(rightValue);

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
            ArgumentGuard.NotNull(relationship, nameof(relationship));
            ArgumentGuard.NotNull(rightResourceIds, nameof(rightResourceIds));

            ResourceContext rightResourceContext = _resourceGraph.GetResourceContext(relationship.RightType);
            AttrAttribute rightIdAttribute = GetIdAttribute(rightResourceContext);

            object[] typedIds = rightResourceIds.Select(resource => resource.GetTypedId()).ToArray();

            FilterExpression baseFilter = GetFilter(Array.Empty<QueryExpression>(), rightResourceContext);
            FilterExpression filter = CreateFilterByIds(typedIds, rightIdAttribute, baseFilter);

            return new QueryLayer(rightResourceContext)
            {
                Include = IncludeExpression.Empty,
                Filter = filter,
                Projection = new Dictionary<ResourceFieldAttribute, QueryLayer>
                {
                    [rightIdAttribute] = null
                }
            };
        }

        /// <inheritdoc />
        public QueryLayer ComposeForHasMany<TId>(HasManyAttribute hasManyRelationship, TId leftId, ICollection<IIdentifiable> rightResourceIds)
        {
            ArgumentGuard.NotNull(hasManyRelationship, nameof(hasManyRelationship));
            ArgumentGuard.NotNull(rightResourceIds, nameof(rightResourceIds));

            ResourceContext leftResourceContext = _resourceGraph.GetResourceContext(hasManyRelationship.LeftType);
            AttrAttribute leftIdAttribute = GetIdAttribute(leftResourceContext);

            ResourceContext rightResourceContext = _resourceGraph.GetResourceContext(hasManyRelationship.RightType);
            AttrAttribute rightIdAttribute = GetIdAttribute(rightResourceContext);
            object[] rightTypedIds = rightResourceIds.Select(resource => resource.GetTypedId()).ToArray();

            FilterExpression leftFilter = CreateFilterByIds(leftId.AsArray(), leftIdAttribute, null);
            FilterExpression rightFilter = CreateFilterByIds(rightTypedIds, rightIdAttribute, null);

            return new QueryLayer(leftResourceContext)
            {
                Include = new IncludeExpression(ImmutableHashSet.Create(new IncludeElementExpression(hasManyRelationship))),
                Filter = leftFilter,
                Projection = new Dictionary<ResourceFieldAttribute, QueryLayer>
                {
                    [hasManyRelationship] = new(rightResourceContext)
                    {
                        Filter = rightFilter,
                        Projection = new Dictionary<ResourceFieldAttribute, QueryLayer>
                        {
                            [rightIdAttribute] = null
                        }
                    },
                    [leftIdAttribute] = null
                }
            };
        }

        protected virtual IImmutableSet<IncludeElementExpression> GetIncludeElements(IImmutableSet<IncludeElementExpression> includeElements,
            ResourceContext resourceContext)
        {
            ArgumentGuard.NotNull(resourceContext, nameof(resourceContext));

            return _resourceDefinitionAccessor.OnApplyIncludes(resourceContext.ResourceType, includeElements);
        }

        protected virtual FilterExpression GetFilter(IReadOnlyCollection<QueryExpression> expressionsInScope, ResourceContext resourceContext)
        {
            ArgumentGuard.NotNull(expressionsInScope, nameof(expressionsInScope));
            ArgumentGuard.NotNull(resourceContext, nameof(resourceContext));

            ImmutableArray<FilterExpression> filters = expressionsInScope.OfType<FilterExpression>().ToImmutableArray();
            FilterExpression filter = filters.Length > 1 ? new LogicalExpression(LogicalOperator.And, filters) : filters.FirstOrDefault();

            return _resourceDefinitionAccessor.OnApplyFilter(resourceContext.ResourceType, filter);
        }

        protected virtual SortExpression GetSort(IReadOnlyCollection<QueryExpression> expressionsInScope, ResourceContext resourceContext)
        {
            ArgumentGuard.NotNull(expressionsInScope, nameof(expressionsInScope));
            ArgumentGuard.NotNull(resourceContext, nameof(resourceContext));

            SortExpression sort = expressionsInScope.OfType<SortExpression>().FirstOrDefault();

            sort = _resourceDefinitionAccessor.OnApplySort(resourceContext.ResourceType, sort);

            if (sort == null)
            {
                AttrAttribute idAttribute = GetIdAttribute(resourceContext);
                var idAscendingSort = new SortElementExpression(new ResourceFieldChainExpression(idAttribute), true);
                sort = new SortExpression(ImmutableArray.Create(idAscendingSort));
            }

            return sort;
        }

        protected virtual PaginationExpression GetPagination(IReadOnlyCollection<QueryExpression> expressionsInScope, ResourceContext resourceContext)
        {
            ArgumentGuard.NotNull(expressionsInScope, nameof(expressionsInScope));
            ArgumentGuard.NotNull(resourceContext, nameof(resourceContext));

            PaginationExpression pagination = expressionsInScope.OfType<PaginationExpression>().FirstOrDefault();

            pagination = _resourceDefinitionAccessor.OnApplyPagination(resourceContext.ResourceType, pagination);

            pagination ??= new PaginationExpression(PageNumber.ValueOne, _options.DefaultPageSize);

            return pagination;
        }

        protected virtual IDictionary<ResourceFieldAttribute, QueryLayer> GetProjectionForSparseAttributeSet(ResourceContext resourceContext)
        {
            ArgumentGuard.NotNull(resourceContext, nameof(resourceContext));

            IImmutableSet<ResourceFieldAttribute> fieldSet = _sparseFieldSetCache.GetSparseFieldSetForQuery(resourceContext);

            if (!fieldSet.Any())
            {
                return null;
            }

            HashSet<AttrAttribute> attributeSet = fieldSet.OfType<AttrAttribute>().ToHashSet();
            AttrAttribute idAttribute = GetIdAttribute(resourceContext);
            attributeSet.Add(idAttribute);

            return attributeSet.ToDictionary(key => (ResourceFieldAttribute)key, _ => (QueryLayer)null);
        }

        private static AttrAttribute GetIdAttribute(ResourceContext resourceContext)
        {
            return resourceContext.GetAttributeByPropertyName(nameof(Identifiable.Id));
        }
    }
}
