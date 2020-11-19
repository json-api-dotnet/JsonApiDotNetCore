using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Internal
{
    /// <inheritdoc />
    public class QueryLayerComposer : IQueryLayerComposer
    {
        private readonly IEnumerable<IQueryConstraintProvider> _constraintProviders;
        private readonly IResourceContextProvider _resourceContextProvider;
        private readonly IResourceDefinitionAccessor _resourceDefinitionAccessor;
        private readonly IJsonApiOptions _options;
        private readonly IPaginationContext _paginationContext;
        private readonly ITargetedFields _targetedFields;

        public QueryLayerComposer(
            IEnumerable<IQueryConstraintProvider> constraintProviders,
            IResourceContextProvider resourceContextProvider,
            IResourceDefinitionAccessor resourceDefinitionAccessor,
            IJsonApiOptions options,
            IPaginationContext paginationContext,
            ITargetedFields targetedFields)
        {
            _constraintProviders = constraintProviders ?? throw new ArgumentNullException(nameof(constraintProviders));
            _resourceContextProvider = resourceContextProvider ?? throw new ArgumentNullException(nameof(resourceContextProvider));
            _resourceDefinitionAccessor = resourceDefinitionAccessor ?? throw new ArgumentNullException(nameof(resourceDefinitionAccessor));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _paginationContext = paginationContext ?? throw new ArgumentNullException(nameof(paginationContext));
            _targetedFields = targetedFields ?? throw new ArgumentNullException(nameof(targetedFields));
        }

        /// <inheritdoc />
        public FilterExpression GetTopFilterFromConstraints()
        {
            var constraints = _constraintProviders.SelectMany(provider => provider.GetConstraints()).ToArray();

            var topFilters = constraints
                .Where(constraint => constraint.Scope == null)
                .Select(constraint => constraint.Expression)
                .OfType<FilterExpression>()
                .ToArray();

            if (topFilters.Length > 1)
            {
                return new LogicalExpression(LogicalOperator.And, topFilters);
            }

            return topFilters.Length == 1 ? topFilters[0] : null;
        }

        /// <inheritdoc />
        public QueryLayer ComposeFromConstraints(ResourceContext requestResource)
        {
            if (requestResource == null) throw new ArgumentNullException(nameof(requestResource));

            var constraints = _constraintProviders.SelectMany(provider => provider.GetConstraints()).ToArray();

            var topLayer = ComposeTopLayer(constraints, requestResource);
            topLayer.Include = ComposeChildren(topLayer, constraints);

            return topLayer;
        }

        private QueryLayer ComposeTopLayer(IEnumerable<ExpressionInScope> constraints, ResourceContext resourceContext)
        {
            var expressionsInTopScope = constraints
                .Where(constraint => constraint.Scope == null)
                .Select(constraint => constraint.Expression)
                .ToArray();

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
                Projection = GetSparseFieldSetProjection(expressionsInTopScope, resourceContext)
            };
        }

        private IncludeExpression ComposeChildren(QueryLayer topLayer, ICollection<ExpressionInScope> constraints)
        {
            var include = constraints
                .Where(constraint => constraint.Scope == null)
                .Select(constraint => constraint.Expression)
                .OfType<IncludeExpression>()
                .FirstOrDefault() ?? IncludeExpression.Empty;

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

                    var expressionsInCurrentScope = constraints
                        .Where(constraint =>
                            constraint.Scope != null && constraint.Scope.Fields.SequenceEqual(relationshipChain))
                        .Select(constraint => constraint.Expression)
                        .ToArray();

                    var resourceContext =
                        _resourceContextProvider.GetResourceContext(includeElement.Relationship.RightType);

                    var child = new QueryLayer(resourceContext)
                    {
                        Filter = GetFilter(expressionsInCurrentScope, resourceContext),
                        Sort = GetSort(expressionsInCurrentScope, resourceContext),
                        Pagination = ((JsonApiOptions)_options).DisableChildrenPagination
                            ? null
                            : GetPagination(expressionsInCurrentScope, resourceContext),
                        Projection = GetSparseFieldSetProjection(expressionsInCurrentScope, resourceContext)
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
            var newIncludeElements = new List<IncludeElementExpression>(includeElements);

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
            if (resourceContext == null) throw new ArgumentNullException(nameof(resourceContext));

            var idAttribute = GetIdAttribute(resourceContext);

            var queryLayer = ComposeFromConstraints(resourceContext);
            queryLayer.Sort = null;
            queryLayer.Pagination = null;
            queryLayer.Filter = CreateFilterByIds(new[] {id}, idAttribute, queryLayer.Filter);

            if (fieldSelection == TopFieldSelection.OnlyIdAttribute)
            {
                queryLayer.Projection = new Dictionary<ResourceFieldAttribute, QueryLayer>
                {
                    [idAttribute] = null
                };
            }
            else if (fieldSelection == TopFieldSelection.WithAllAttributes && queryLayer.Projection != null)
            {
                // Discard any top-level ?fields= or attribute exclusions from resource definition, because we need the full database row.
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
            if (secondaryResourceContext == null) throw new ArgumentNullException(nameof(secondaryResourceContext));

            var secondaryLayer = ComposeFromConstraints(secondaryResourceContext);
            secondaryLayer.Projection = GetProjectionForRelationship(secondaryResourceContext);
            secondaryLayer.Include = null;

            return secondaryLayer;
        }

        private IDictionary<ResourceFieldAttribute, QueryLayer> GetProjectionForRelationship(ResourceContext secondaryResourceContext)
        {
            var secondaryIdAttribute = GetIdAttribute(secondaryResourceContext);
            var sparseFieldSet = new SparseFieldSetExpression(new[] {secondaryIdAttribute});

            var secondaryProjection = GetSparseFieldSetProjection(new[] {sparseFieldSet}, secondaryResourceContext) ?? new Dictionary<ResourceFieldAttribute, QueryLayer>();
            secondaryProjection[secondaryIdAttribute] = null;

            return secondaryProjection;
        }

        /// <inheritdoc />
        public QueryLayer WrapLayerForSecondaryEndpoint<TId>(QueryLayer secondaryLayer, ResourceContext primaryResourceContext, TId primaryId, RelationshipAttribute secondaryRelationship)
        {
            if (secondaryLayer == null) throw new ArgumentNullException(nameof(secondaryLayer));
            if (primaryResourceContext == null) throw new ArgumentNullException(nameof(primaryResourceContext));
            if (secondaryRelationship == null) throw new ArgumentNullException(nameof(secondaryRelationship));

            var innerInclude = secondaryLayer.Include;
            secondaryLayer.Include = null;

            var primaryIdAttribute = GetIdAttribute(primaryResourceContext);
            var sparseFieldSet = new SparseFieldSetExpression(new[] {primaryIdAttribute});

            var primaryProjection = GetSparseFieldSetProjection(new[] {sparseFieldSet}, primaryResourceContext) ?? new Dictionary<ResourceFieldAttribute, QueryLayer>();
            primaryProjection[secondaryRelationship] = secondaryLayer;
            primaryProjection[primaryIdAttribute] = null;

            var primaryFilter = GetFilter(Array.Empty<QueryExpression>(), primaryResourceContext);

            return new QueryLayer(primaryResourceContext)
            {
                Include = RewriteIncludeForSecondaryEndpoint(innerInclude, secondaryRelationship),
                Filter = CreateFilterByIds(new[] {primaryId}, primaryIdAttribute, primaryFilter),
                Projection = primaryProjection
            };
        }

        private IncludeExpression RewriteIncludeForSecondaryEndpoint(IncludeExpression relativeInclude, RelationshipAttribute secondaryRelationship)
        {
            var parentElement = relativeInclude != null
                ? new IncludeElementExpression(secondaryRelationship, relativeInclude.Elements)
                : new IncludeElementExpression(secondaryRelationship);

            return new IncludeExpression(new[] {parentElement});
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

            return filter == null
                ? existingFilter
                : existingFilter == null
                    ? filter
                    : new LogicalExpression(LogicalOperator.And, new[] {filter, existingFilter});
        }

        /// <inheritdoc />
        public QueryLayer ComposeForUpdate<TId>(TId id, ResourceContext primaryResource)
        {
            if (primaryResource == null) throw new ArgumentNullException(nameof(primaryResource));

            var includeElements = _targetedFields.Relationships
                .Select(relationship => new IncludeElementExpression(relationship)).ToArray();

            var primaryIdAttribute = GetIdAttribute(primaryResource);

            var primaryLayer = ComposeTopLayer(Array.Empty<ExpressionInScope>(), primaryResource);
            primaryLayer.Include = includeElements.Any() ? new IncludeExpression(includeElements) : IncludeExpression.Empty;
            primaryLayer.Sort = null;
            primaryLayer.Pagination = null;
            primaryLayer.Filter = CreateFilterByIds(new[] {id}, primaryIdAttribute, primaryLayer.Filter);
            primaryLayer.Projection = null;

            return primaryLayer;
        }

        /// <inheritdoc />
        public IEnumerable<(QueryLayer, RelationshipAttribute)> ComposeForGetTargetedSecondaryResourceIds(IIdentifiable primaryResource)
        {
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
        public QueryLayer ComposeForHasManyThrough<TId>(HasManyThroughAttribute hasManyThroughRelationship, TId leftId, ICollection<IIdentifiable> rightResourceIds)
        {
            var leftResourceContext = _resourceContextProvider.GetResourceContext(hasManyThroughRelationship.LeftType);
            var leftIdAttribute = GetIdAttribute(leftResourceContext);

            var rightResourceContext = _resourceContextProvider.GetResourceContext(hasManyThroughRelationship.RightType);
            var rightIdAttribute = GetIdAttribute(rightResourceContext);
            var rightTypedIds = rightResourceIds.Select(resource => resource.GetTypedId()).ToArray();

            var leftFilter = CreateFilterByIds(new[] {leftId}, leftIdAttribute, null);
            var rightFilter = CreateFilterByIds(rightTypedIds, rightIdAttribute, null);

            return new QueryLayer(leftResourceContext)
            {
                Include = new IncludeExpression(new[] {new IncludeElementExpression(hasManyThroughRelationship)}),
                Filter = leftFilter,
                Projection = new Dictionary<ResourceFieldAttribute, QueryLayer>
                {
                    [hasManyThroughRelationship] = new QueryLayer(rightResourceContext)
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
            if (resourceContext == null) throw new ArgumentNullException(nameof(resourceContext));

            includeElements = _resourceDefinitionAccessor.OnApplyIncludes(resourceContext.ResourceType, includeElements);
            return includeElements;
        }

        protected virtual FilterExpression GetFilter(IReadOnlyCollection<QueryExpression> expressionsInScope, ResourceContext resourceContext)
        {
            if (expressionsInScope == null) throw new ArgumentNullException(nameof(expressionsInScope));
            if (resourceContext == null) throw new ArgumentNullException(nameof(resourceContext));

            var filters = expressionsInScope.OfType<FilterExpression>().ToArray();

            var filter = filters.Length > 1
                ? new LogicalExpression(LogicalOperator.And, filters)
                : filters.FirstOrDefault();

            return _resourceDefinitionAccessor.OnApplyFilter(resourceContext.ResourceType, filter);
        }

        protected virtual SortExpression GetSort(IReadOnlyCollection<QueryExpression> expressionsInScope, ResourceContext resourceContext)
        {
            if (expressionsInScope == null) throw new ArgumentNullException(nameof(expressionsInScope));
            if (resourceContext == null) throw new ArgumentNullException(nameof(resourceContext));

            var sort = expressionsInScope.OfType<SortExpression>().FirstOrDefault();

            sort = _resourceDefinitionAccessor.OnApplySort(resourceContext.ResourceType, sort);

            if (sort == null)
            {
                var idAttribute = GetIdAttribute(resourceContext);
                sort = new SortExpression(new[] {new SortElementExpression(new ResourceFieldChainExpression(idAttribute), true)});
            }

            return sort;
        }

        protected virtual PaginationExpression GetPagination(IReadOnlyCollection<QueryExpression> expressionsInScope, ResourceContext resourceContext)
        {
            if (expressionsInScope == null) throw new ArgumentNullException(nameof(expressionsInScope));
            if (resourceContext == null) throw new ArgumentNullException(nameof(resourceContext));

            var pagination = expressionsInScope.OfType<PaginationExpression>().FirstOrDefault();

            pagination = _resourceDefinitionAccessor.OnApplyPagination(resourceContext.ResourceType, pagination);

            pagination ??= new PaginationExpression(PageNumber.ValueOne, _options.DefaultPageSize);

            return pagination;
        }

        protected virtual IDictionary<ResourceFieldAttribute, QueryLayer> GetSparseFieldSetProjection(IReadOnlyCollection<QueryExpression> expressionsInScope, ResourceContext resourceContext)
        {
            if (expressionsInScope == null) throw new ArgumentNullException(nameof(expressionsInScope));
            if (resourceContext == null) throw new ArgumentNullException(nameof(resourceContext));

            var attributes = expressionsInScope.OfType<SparseFieldSetExpression>().SelectMany(sparseFieldSet => sparseFieldSet.Attributes).ToHashSet();

            var tempExpression = attributes.Any() ? new SparseFieldSetExpression(attributes) : null;
            tempExpression = _resourceDefinitionAccessor.OnApplySparseFieldSet(resourceContext.ResourceType, tempExpression);

            attributes = tempExpression == null ? new HashSet<AttrAttribute>() : tempExpression.Attributes.ToHashSet();

            if (!attributes.Any())
            {
                return null;
            }

            var idAttribute = GetIdAttribute(resourceContext);
            attributes.Add(idAttribute);

            return attributes.Cast<ResourceFieldAttribute>().ToDictionary(key => key, value => (QueryLayer)null);
        }

        private static AttrAttribute GetIdAttribute(ResourceContext resourceContext)
        {
            return resourceContext.Attributes.Single(attr => attr.Property.Name == nameof(Identifiable.Id));
        }
    }
}
