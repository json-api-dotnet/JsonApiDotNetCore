using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Internal
{
    /// <inheritdoc/>
    public class QueryLayerComposer : IQueryLayerComposer
    {
        private readonly IEnumerable<IQueryConstraintProvider> _constraintProviders;
        private readonly IResourceContextProvider _resourceContextProvider;
        private readonly IResourceDefinitionProvider _resourceDefinitionProvider;
        private readonly IJsonApiOptions _options;
        private readonly IPaginationContext _paginationContext;

        public QueryLayerComposer(
            IEnumerable<IQueryConstraintProvider> constraintProviders,
            IResourceContextProvider resourceContextProvider,
            IResourceDefinitionProvider resourceDefinitionProvider, 
            IJsonApiOptions options,
            IPaginationContext paginationContext)
        {
            _constraintProviders = constraintProviders ?? throw new ArgumentNullException(nameof(constraintProviders));
            _resourceContextProvider = resourceContextProvider ?? throw new ArgumentNullException(nameof(resourceContextProvider));
            _resourceDefinitionProvider = resourceDefinitionProvider ?? throw new ArgumentNullException(nameof(resourceDefinitionProvider));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _paginationContext = paginationContext ?? throw new ArgumentNullException(nameof(paginationContext));
        }

        /// <inheritdoc/>
        public FilterExpression GetTopFilter()
        {
            var constraints = _constraintProviders.SelectMany(p => p.GetConstraints()).ToArray();

            var topFilters = constraints
                .Where(c => c.Scope == null)
                .Select(c => c.Expression)
                .OfType<FilterExpression>()
                .ToArray();

            if (!topFilters.Any())
            {
                return null;
            }

            if (topFilters.Length == 1)
            {
                return topFilters[0];
            }

            return new LogicalExpression(LogicalOperator.And, topFilters);
        }

        /// <inheritdoc/>
        public QueryLayer Compose(ResourceContext requestResource)
        {
            if (requestResource == null)
            {
                throw new ArgumentNullException(nameof(requestResource));
            }

            var constraints = _constraintProviders.SelectMany(p => p.GetConstraints()).ToArray();

            var topLayer = ComposeTopLayer(constraints, requestResource);
            topLayer.Include = ComposeChildren(topLayer, constraints);

            return topLayer;
        }

        private QueryLayer ComposeTopLayer(IEnumerable<ExpressionInScope> constraints, ResourceContext resourceContext)
        {
            var expressionsInTopScope = constraints
                .Where(c => c.Scope == null)
                .Select(expressionInScope => expressionInScope.Expression)
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

        private IncludeExpression ComposeChildren(QueryLayer topLayer, ExpressionInScope[] constraints)
        {
            var include = constraints
                .Where(c => c.Scope == null)
                .Select(expressionInScope => expressionInScope.Expression).OfType<IncludeExpression>()
                .FirstOrDefault() ?? IncludeExpression.Empty;

            var includeElements =
                ProcessIncludeSet(include.Elements, topLayer, new List<RelationshipAttribute>(), constraints);

            return !ReferenceEquals(includeElements, include.Elements)
                ? includeElements.Any() ? new IncludeExpression(includeElements) : IncludeExpression.Empty
                : include;
        }

        private IReadOnlyCollection<IncludeElementExpression> ProcessIncludeSet(IReadOnlyCollection<IncludeElementExpression> includeElements, 
            QueryLayer parentLayer, ICollection<RelationshipAttribute> parentRelationshipChain, ExpressionInScope[] constraints)
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
                        .Where(c => c.Scope != null && c.Scope.Fields.SequenceEqual(relationshipChain))
                        .Select(expressionInScope => expressionInScope.Expression)
                        .ToArray();

                    var resourceContext =
                        _resourceContextProvider.GetResourceContext(includeElement.Relationship.RightType);

                    var child = new QueryLayer(resourceContext)
                    {
                        Filter = GetFilter(expressionsInCurrentScope, resourceContext),
                        Sort = GetSort(expressionsInCurrentScope, resourceContext),
                        Pagination = ((JsonApiOptions) _options).DisableChildrenPagination
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

        private static IReadOnlyCollection<IncludeElementExpression> ApplyIncludeElementUpdates(IReadOnlyCollection<IncludeElementExpression> includeElements,
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

        protected virtual IReadOnlyCollection<IncludeElementExpression> GetIncludeElements(IReadOnlyCollection<IncludeElementExpression> includeElements, ResourceContext resourceContext)
        {
            if (resourceContext == null) throw new ArgumentNullException(nameof(resourceContext));

            var resourceDefinition = _resourceDefinitionProvider.Get(resourceContext.ResourceType);
            if (resourceDefinition != null)
            {
                includeElements = resourceDefinition.OnApplyIncludes(includeElements);
            }

            return includeElements;
        }

        protected virtual FilterExpression GetFilter(IReadOnlyCollection<QueryExpression> expressionsInScope, ResourceContext resourceContext)
        {
            if (expressionsInScope == null) throw new ArgumentNullException(nameof(expressionsInScope));
            if (resourceContext == null) throw new ArgumentNullException(nameof(resourceContext));

            var filters = expressionsInScope.OfType<FilterExpression>().ToArray();
            var filter = filters.Length > 1 ? new LogicalExpression(LogicalOperator.And, filters) : filters.FirstOrDefault();

            var resourceDefinition = _resourceDefinitionProvider.Get(resourceContext.ResourceType);
            if (resourceDefinition != null)
            {
                filter = resourceDefinition.OnApplyFilter(filter);
            }

            return filter;
        }

        protected virtual SortExpression GetSort(IReadOnlyCollection<QueryExpression> expressionsInScope, ResourceContext resourceContext)
        {
            if (expressionsInScope == null) throw new ArgumentNullException(nameof(expressionsInScope));
            if (resourceContext == null) throw new ArgumentNullException(nameof(resourceContext));

            var sort = expressionsInScope.OfType<SortExpression>().FirstOrDefault();

            var resourceDefinition = _resourceDefinitionProvider.Get(resourceContext.ResourceType);
            if (resourceDefinition != null)
            {
                sort = resourceDefinition.OnApplySort(sort);
            }

            if (sort == null)
            {
                var idAttribute = resourceContext.Attributes.Single(x => x.Property.Name == nameof(Identifiable.Id));
                sort = new SortExpression(new[] {new SortElementExpression(new ResourceFieldChainExpression(idAttribute), true)});
            }

            return sort;
        }

        protected virtual PaginationExpression GetPagination(IReadOnlyCollection<QueryExpression> expressionsInScope, ResourceContext resourceContext)
        {
            if (expressionsInScope == null) throw new ArgumentNullException(nameof(expressionsInScope));
            if (resourceContext == null) throw new ArgumentNullException(nameof(resourceContext));

            var pagination = expressionsInScope.OfType<PaginationExpression>().FirstOrDefault();

            var resourceDefinition = _resourceDefinitionProvider.Get(resourceContext.ResourceType);
            if (resourceDefinition != null)
            {
                pagination = resourceDefinition.OnApplyPagination(pagination);
            }

            pagination ??= new PaginationExpression(PageNumber.ValueOne, _options.DefaultPageSize);

            return pagination;
        }

        protected virtual IDictionary<ResourceFieldAttribute, QueryLayer> GetSparseFieldSetProjection(IReadOnlyCollection<QueryExpression> expressionsInScope, ResourceContext resourceContext)
        {
            if (expressionsInScope == null) throw new ArgumentNullException(nameof(expressionsInScope));
            if (resourceContext == null) throw new ArgumentNullException(nameof(resourceContext));

            var attributes = expressionsInScope.OfType<SparseFieldSetExpression>().SelectMany(sparseFieldSet => sparseFieldSet.Attributes).ToHashSet();

            var resourceDefinition = _resourceDefinitionProvider.Get(resourceContext.ResourceType);
            if (resourceDefinition != null)
            {
                var tempExpression = attributes.Any() ? new SparseFieldSetExpression(attributes) : null;
                tempExpression = resourceDefinition.OnApplySparseFieldSet(tempExpression);

                attributes = tempExpression == null ? new HashSet<AttrAttribute>() : tempExpression.Attributes.ToHashSet();
            }

            if (!attributes.Any())
            {
                return null;
            }

            var idAttribute = resourceContext.Attributes.Single(x => x.Property.Name == nameof(Identifiable.Id));
            attributes.Add(idAttribute);

            return attributes.Cast<ResourceFieldAttribute>().ToDictionary(key => key, value => (QueryLayer) null);
        }
    }
}
