using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Internal
{
    /// <inheritdoc />
    [PublicAPI]
    public class QueryLayerComposer : IQueryLayerComposer
    {
        private readonly IEnumerable<IQueryConstraintProvider> _constraintProviders;
        private readonly IResourceContextProvider _resourceContextProvider;
        private readonly IResourceDefinitionAccessor _resourceDefinitionAccessor;
        private readonly IJsonApiOptions _options;
        private readonly IPaginationContext _paginationContext;
        private readonly ITargetedFields _targetedFields;
        private readonly SparseFieldSetCache _sparseFieldSetCache;

        public QueryLayerComposer(
            IEnumerable<IQueryConstraintProvider> constraintProviders,
            IResourceContextProvider resourceContextProvider,
            IResourceDefinitionAccessor resourceDefinitionAccessor,
            IJsonApiOptions options,
            IPaginationContext paginationContext,
            ITargetedFields targetedFields)
        {
            ArgumentGuard.NotNull(constraintProviders, nameof(constraintProviders));
            ArgumentGuard.NotNull(resourceContextProvider, nameof(resourceContextProvider));
            ArgumentGuard.NotNull(resourceDefinitionAccessor, nameof(resourceDefinitionAccessor));
            ArgumentGuard.NotNull(options, nameof(options));
            ArgumentGuard.NotNull(paginationContext, nameof(paginationContext));
            ArgumentGuard.NotNull(targetedFields, nameof(targetedFields));

            _constraintProviders = constraintProviders;
            _resourceContextProvider = resourceContextProvider;
            _resourceDefinitionAccessor = resourceDefinitionAccessor;
            _options = options;
            _paginationContext = paginationContext;
            _targetedFields = targetedFields;
            _sparseFieldSetCache = new SparseFieldSetCache(_constraintProviders, resourceDefinitionAccessor);
        }

        /// <inheritdoc />
        public FilterExpression GetTopFilterFromConstraints(ResourceContext resourceContext)
        {
            var constraints = _constraintProviders.SelectMany(provider => provider.GetConstraints()).ToArray();

            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:keep_existing_linebreaks true

            var filtersInTopScope = constraints
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

            var constraints = _constraintProviders.SelectMany(provider => provider.GetConstraints()).ToArray();

            var topLayer = ComposeTopLayer(constraints, requestResource);
            topLayer.Include = ComposeChildren(topLayer, constraints);

            return topLayer;
        }

        private QueryLayer ComposeTopLayer(IEnumerable<ExpressionInScope> constraints, ResourceContext resourceContext)
        {
            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:keep_existing_linebreaks true

            var expressionsInTopScope = constraints
                .Where(constraint => constraint.Scope == null)
                .Select(constraint => constraint.Expression)
                .ToArray();

            // @formatter:keep_existing_linebreaks restore
            // @formatter:wrap_chained_method_calls restore

            var topPagination = GetPagination(expressionsInTopScope, resourceContext);
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
            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:keep_existing_linebreaks true

            var include = constraints
                .Where(constraint => constraint.Scope == null)
                .Select(constraint => constraint.Expression)
                .OfType<IncludeExpression>()
                .FirstOrDefault() ?? IncludeExpression.Empty;

            // @formatter:keep_existing_linebreaks restore
            // @formatter:wrap_chained_method_calls restore

            var includeElements =
                ProcessIncludeSet(include.Elements, topLayer, new List<RelationshipAttribute>(), constraints);

            return !ReferenceEquals(includeElements, include.Elements)
                ? includeElements.Any() ? new IncludeExpression(includeElements) : IncludeExpression.Empty
                : include;
        }

        private IReadOnlyCollection<IncludeElementExpression> ProcessIncludeSet(IReadOnlyCollection<IncludeElementExpression> includeElements,
            QueryLayer parentLayer, ICollection<RelationshipAttribute> parentRelationshipChain, ICollection<ExpressionInScope> constraints)
        {
            includeElements = GetIncludeElements(includeElements, parentLayer.ResourceContext) ?? Array.Empty<IncludeElementExpression>();

            var updatesInChildren = new Dictionary<IncludeElementExpression, IReadOnlyCollection<IncludeElementExpression>>();

            foreach (var includeElement in includeElements)
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

                    var expressionsInCurrentScope = constraints
                        .Where(constraint =>
                            constraint.Scope != null && constraint.Scope.Fields.SequenceEqual(relationshipChain))
                        .Select(constraint => constraint.Expression)
                        .ToArray();

                    // @formatter:keep_existing_linebreaks restore
                    // @formatter:wrap_chained_method_calls restore

                    var resourceContext =
                        _resourceContextProvider.GetResourceContext(includeElement.Relationship.RightType);

                    var child = new QueryLayer(resourceContext)
                    {
                        Filter = GetFilter(expressionsInCurrentScope, resourceContext),
                        Sort = GetSort(expressionsInCurrentScope, resourceContext),
                        Pagination = ((JsonApiOptions)_options).DisableChildrenPagination
                            ? null
                            : GetPagination(expressionsInCurrentScope, resourceContext),
                        Projection = GetProjectionForSparseAttributeSet(resourceContext)
                    };

                    parentLayer.Projection.Add(includeElement.Relationship, child);

                    if (includeElement.Children.Any())
                    {
                        var updatedChildren = ProcessIncludeSet(includeElement.Children, child, relationshipChain, constraints);

                        if (!ReferenceEquals(includeElement.Children, updatedChildren))
                        {
                            updatesInChildren.Add(includeElement, updatedChildren);
                        }
                    }
                }
            }

            return !updatesInChildren.Any() ? includeElements : ApplyIncludeElementUpdates(includeElements, updatesInChildren);
        }

        private static IReadOnlyCollection<IncludeElementExpression> ApplyIncludeElementUpdates(IEnumerable<IncludeElementExpression> includeElements,
            IDictionary<IncludeElementExpression, IReadOnlyCollection<IncludeElementExpression>> updatesInChildren)
        {
            var newIncludeElements = includeElements.ToList();

            foreach (var (existingElement, updatedChildren) in updatesInChildren)
            {
                var existingIndex = newIncludeElements.IndexOf(existingElement);
                newIncludeElements[existingIndex] = new IncludeElementExpression(existingElement.Relationship, updatedChildren);
            }

            return newIncludeElements;
        }

        /// <inheritdoc />
        public QueryLayer ComposeForGetById<TId>(TId id, ResourceContext resourceContext, TopFieldSelection fieldSelection)
        {
            ArgumentGuard.NotNull(resourceContext, nameof(resourceContext));

            var idAttribute = GetIdAttribute(resourceContext);

            var queryLayer = ComposeFromConstraints(resourceContext);
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

            var secondaryLayer = ComposeFromConstraints(secondaryResourceContext);
            secondaryLayer.Projection = GetProjectionForRelationship(secondaryResourceContext);
            secondaryLayer.Include = null;

            return secondaryLayer;
        }

        private IDictionary<ResourceFieldAttribute, QueryLayer> GetProjectionForRelationship(ResourceContext secondaryResourceContext)
        {
            var secondaryAttributeSet = _sparseFieldSetCache.GetIdAttributeSetForRelationshipQuery(secondaryResourceContext);

            return secondaryAttributeSet.ToDictionary(key => (ResourceFieldAttribute)key, value => (QueryLayer)null);
        }

        /// <inheritdoc />
        public QueryLayer WrapLayerForSecondaryEndpoint<TId>(QueryLayer secondaryLayer, ResourceContext primaryResourceContext, TId primaryId, RelationshipAttribute secondaryRelationship)
        {
            ArgumentGuard.NotNull(secondaryLayer, nameof(secondaryLayer));
            ArgumentGuard.NotNull(primaryResourceContext, nameof(primaryResourceContext));
            ArgumentGuard.NotNull(secondaryRelationship, nameof(secondaryRelationship));

            var innerInclude = secondaryLayer.Include;
            secondaryLayer.Include = null;

            var primaryAttributeSet = _sparseFieldSetCache.GetIdAttributeSetForRelationshipQuery(primaryResourceContext);
            var primaryProjection = primaryAttributeSet.ToDictionary(key => (ResourceFieldAttribute)key, value => (QueryLayer)null);
            primaryProjection[secondaryRelationship] = secondaryLayer;

            var primaryFilter = GetFilter(Array.Empty<QueryExpression>(), primaryResourceContext);
            var primaryIdAttribute = GetIdAttribute(primaryResourceContext);

            return new QueryLayer(primaryResourceContext)
            {
                Include = RewriteIncludeForSecondaryEndpoint(innerInclude, secondaryRelationship),
                Filter = CreateFilterByIds(primaryId.AsArray(), primaryIdAttribute, primaryFilter),
                Projection = primaryProjection
            };
        }

        private IncludeExpression RewriteIncludeForSecondaryEndpoint(IncludeExpression relativeInclude, RelationshipAttribute secondaryRelationship)
        {
            var parentElement = relativeInclude != null
                ? new IncludeElementExpression(secondaryRelationship, relativeInclude.Elements)
                : new IncludeElementExpression(secondaryRelationship);

            return new IncludeExpression(parentElement.AsArray());
        }

        private FilterExpression CreateFilterByIds<TId>(ICollection<TId> ids, AttrAttribute idAttribute, FilterExpression existingFilter)
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
                var constants = ids.Select(id => new LiteralConstantExpression(id.ToString())).ToList();
                filter = new EqualsAnyOfExpression(idChain, constants);
            }

            // @formatter:keep_existing_linebreaks true

            return filter == null
                ? existingFilter
                : existingFilter == null
                    ? filter
                    : new LogicalExpression(LogicalOperator.And, ArrayFactory.Create(filter, existingFilter));

            // @formatter:keep_existing_linebreaks restore
        }

        /// <inheritdoc />
        public QueryLayer ComposeForUpdate<TId>(TId id, ResourceContext primaryResource)
        {
            ArgumentGuard.NotNull(primaryResource, nameof(primaryResource));

            var includeElements = _targetedFields.Relationships
                .Select(relationship => new IncludeElementExpression(relationship)).ToArray();

            var primaryIdAttribute = GetIdAttribute(primaryResource);

            var primaryLayer = ComposeTopLayer(Array.Empty<ExpressionInScope>(), primaryResource);
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

            foreach (var relationship in _targetedFields.Relationships)
            {
                object rightValue = relationship.GetValue(primaryResource);
                ICollection<IIdentifiable> rightResourceIds = TypeHelper.ExtractResources(rightValue);

                if (rightResourceIds.Any())
                {
                    var queryLayer = ComposeForGetRelationshipRightIds(relationship, rightResourceIds);
                    yield return (queryLayer, relationship);
                }
            }
        }

        /// <inheritdoc />
        public QueryLayer ComposeForGetRelationshipRightIds(RelationshipAttribute relationship, ICollection<IIdentifiable> rightResourceIds)
        {
            ArgumentGuard.NotNull(relationship, nameof(relationship));
            ArgumentGuard.NotNull(rightResourceIds, nameof(rightResourceIds));

            var rightResourceContext = _resourceContextProvider.GetResourceContext(relationship.RightType);
            var rightIdAttribute = GetIdAttribute(rightResourceContext);

            var typedIds = rightResourceIds.Select(resource => resource.GetTypedId()).ToArray();

            var baseFilter = GetFilter(Array.Empty<QueryExpression>(), rightResourceContext);
            var filter = CreateFilterByIds(typedIds, rightIdAttribute, baseFilter);

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

            var leftResourceContext = _resourceContextProvider.GetResourceContext(hasManyRelationship.LeftType);
            var leftIdAttribute = GetIdAttribute(leftResourceContext);

            var rightResourceContext = _resourceContextProvider.GetResourceContext(hasManyRelationship.RightType);
            var rightIdAttribute = GetIdAttribute(rightResourceContext);
            var rightTypedIds = rightResourceIds.Select(resource => resource.GetTypedId()).ToArray();

            var leftFilter = CreateFilterByIds(leftId.AsArray(), leftIdAttribute, null);
            var rightFilter = CreateFilterByIds(rightTypedIds, rightIdAttribute, null);

            return new QueryLayer(leftResourceContext)
            {
                Include = new IncludeExpression(new IncludeElementExpression(hasManyRelationship).AsArray()),
                Filter = leftFilter,
                Projection = new Dictionary<ResourceFieldAttribute, QueryLayer>
                {
                    [hasManyRelationship] = new QueryLayer(rightResourceContext)
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

        protected virtual IReadOnlyCollection<IncludeElementExpression> GetIncludeElements(IReadOnlyCollection<IncludeElementExpression> includeElements, ResourceContext resourceContext)
        {
            ArgumentGuard.NotNull(resourceContext, nameof(resourceContext));

            includeElements = _resourceDefinitionAccessor.OnApplyIncludes(resourceContext.ResourceType, includeElements);
            return includeElements;
        }

        protected virtual FilterExpression GetFilter(IReadOnlyCollection<QueryExpression> expressionsInScope, ResourceContext resourceContext)
        {
            ArgumentGuard.NotNull(expressionsInScope, nameof(expressionsInScope));
            ArgumentGuard.NotNull(resourceContext, nameof(resourceContext));

            var filters = expressionsInScope.OfType<FilterExpression>().ToArray();

            var filter = filters.Length > 1
                ? new LogicalExpression(LogicalOperator.And, filters)
                : filters.FirstOrDefault();

            return _resourceDefinitionAccessor.OnApplyFilter(resourceContext.ResourceType, filter);
        }

        protected virtual SortExpression GetSort(IReadOnlyCollection<QueryExpression> expressionsInScope, ResourceContext resourceContext)
        {
            ArgumentGuard.NotNull(expressionsInScope, nameof(expressionsInScope));
            ArgumentGuard.NotNull(resourceContext, nameof(resourceContext));

            var sort = expressionsInScope.OfType<SortExpression>().FirstOrDefault();

            sort = _resourceDefinitionAccessor.OnApplySort(resourceContext.ResourceType, sort);

            if (sort == null)
            {
                var idAttribute = GetIdAttribute(resourceContext);
                sort = new SortExpression(new SortElementExpression(new ResourceFieldChainExpression(idAttribute), true).AsArray());
            }

            return sort;
        }

        protected virtual PaginationExpression GetPagination(IReadOnlyCollection<QueryExpression> expressionsInScope, ResourceContext resourceContext)
        {
            ArgumentGuard.NotNull(expressionsInScope, nameof(expressionsInScope));
            ArgumentGuard.NotNull(resourceContext, nameof(resourceContext));

            var pagination = expressionsInScope.OfType<PaginationExpression>().FirstOrDefault();

            pagination = _resourceDefinitionAccessor.OnApplyPagination(resourceContext.ResourceType, pagination);

            pagination ??= new PaginationExpression(PageNumber.ValueOne, _options.DefaultPageSize);

            return pagination;
        }

        protected virtual IDictionary<ResourceFieldAttribute, QueryLayer> GetProjectionForSparseAttributeSet(ResourceContext resourceContext)
        {
            ArgumentGuard.NotNull(resourceContext, nameof(resourceContext));

            var fieldSet = _sparseFieldSetCache.GetSparseFieldSetForQuery(resourceContext);
            if (!fieldSet.Any())
            {
                return null;
            }

            var attributeSet = fieldSet.OfType<AttrAttribute>().ToHashSet();
            var idAttribute = GetIdAttribute(resourceContext);
            attributeSet.Add(idAttribute);

            return attributeSet.ToDictionary(key => (ResourceFieldAttribute)key, value => (QueryLayer)null);
        }

        private static AttrAttribute GetIdAttribute(ResourceContext resourceContext)
        {
            return resourceContext.Attributes.Single(attr => attr.Property.Name == nameof(Identifiable.Id));
        }
    }
}
