using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Internal.Queries.Expressions;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Annotation;
using JsonApiDotNetCore.Query;

namespace JsonApiDotNetCore.Internal.Queries
{
    public interface IQueryLayerComposer
    {
        FilterExpression GetTopFilter();
        QueryLayer Compose(ResourceContext requestResource);
    }

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

        public QueryLayer Compose(ResourceContext requestResource)
        {
            if (requestResource == null)
            {
                throw new ArgumentNullException(nameof(requestResource));
            }

            var constraints = _constraintProviders.SelectMany(p => p.GetConstraints()).ToArray();

            var topLayer = ComposeTopLayer(constraints, requestResource);

            ComposeChildren(topLayer, constraints);

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
                Include = GetIncludes(expressionsInTopScope),
                Filter = GetFilter(expressionsInTopScope, resourceContext),
                Sort = GetSort(expressionsInTopScope, resourceContext),
                Pagination = ((JsonApiOptions)_options).DisableTopPagination ? null : topPagination,
                Projection = GetSparseFieldSetProjection(expressionsInTopScope, resourceContext)
            };
        }

        private void ComposeChildren(QueryLayer topLayer, ExpressionInScope[] constraints)
        {
            if (topLayer.Include == null)
            {
                return;
            }

            foreach (var includeChain in topLayer.Include.Chains)
            {
                var currentLayer = topLayer;
                List<RelationshipAttribute> currentScope = new List<RelationshipAttribute>();

                foreach (var relationship in includeChain.Fields.OfType<RelationshipAttribute>())
                {
                    currentScope.Add(relationship);
                    var currentResourceContext = _resourceContextProvider.GetResourceContext(relationship.RightType);

                    currentLayer.Projection ??= new Dictionary<ResourceFieldAttribute, QueryLayer>();

                    if (!currentLayer.Projection.ContainsKey(relationship))
                    {
                        var expressionsInCurrentScope = constraints
                            .Where(c => c.Scope != null && c.Scope.Fields.SequenceEqual(currentScope))
                            .Select(expressionInScope => expressionInScope.Expression)
                            .ToArray();

                        var child = new QueryLayer(currentResourceContext)
                        {
                            Filter = GetFilter(expressionsInCurrentScope, currentResourceContext),
                            Sort = GetSort(expressionsInCurrentScope, currentResourceContext),
                            Pagination = ((JsonApiOptions)_options).DisableChildrenPagination ? null : GetPagination(expressionsInCurrentScope, currentResourceContext),
                            Projection = GetSparseFieldSetProjection(expressionsInCurrentScope, currentResourceContext)
                        };

                        currentLayer.Projection.Add(relationship, child);
                    }

                    currentLayer = currentLayer.Projection[relationship];
                }
            }
        }

        protected virtual IncludeExpression GetIncludes(IEnumerable<QueryExpression> expressionsInScope)
        {
            return expressionsInScope.OfType<IncludeExpression>().FirstOrDefault();
        }

        protected virtual FilterExpression GetFilter(IEnumerable<QueryExpression> expressionsInScope, ResourceContext resourceContext)
        {
            var filters = expressionsInScope.OfType<FilterExpression>().ToArray();
            var filter = filters.Length > 1 ? new LogicalExpression(LogicalOperator.And, filters) : filters.FirstOrDefault();

            var resourceDefinition = _resourceDefinitionProvider.Get(resourceContext.ResourceType);
            if (resourceDefinition != null)
            {
                filter = resourceDefinition.OnApplyFilter(filter);
            }

            return filter;
        }

        protected virtual SortExpression GetSort(IEnumerable<QueryExpression> expressionsInScope, ResourceContext resourceContext)
        {
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

        protected virtual PaginationExpression GetPagination(IEnumerable<QueryExpression> expressionsInScope, ResourceContext resourceContext)
        {
            var pagination = expressionsInScope.OfType<PaginationExpression>().FirstOrDefault();

            var resourceDefinition = _resourceDefinitionProvider.Get(resourceContext.ResourceType);
            if (resourceDefinition != null)
            {
                pagination = resourceDefinition.OnApplyPagination(pagination);
            }

            pagination ??= new PaginationExpression(PageNumber.ValueOne, _options.DefaultPageSize);

            return pagination;
        }

        protected virtual IDictionary<ResourceFieldAttribute, QueryLayer> GetSparseFieldSetProjection(IEnumerable<QueryExpression> expressionsInScope, ResourceContext resourceContext)
        {
            var attributes = expressionsInScope.OfType<SparseFieldSetExpression>().SelectMany(sparseFieldSet => sparseFieldSet.Attributes).ToHashSet();

            var resourceDefinition = _resourceDefinitionProvider.Get(resourceContext.ResourceType);
            if (resourceDefinition != null)
            {
                var tempExpression = attributes.Any() ? new SparseFieldSetExpression(attributes) : null;
                tempExpression = resourceDefinition.OnApplySparseFieldSet(tempExpression);

                attributes = tempExpression == null ? new HashSet<AttrAttribute>() : tempExpression.Attributes.ToHashSet();
            }

            if (attributes.Any())
            {
                var idAttribute = resourceContext.Attributes.Single(x => x.Property.Name == nameof(Identifiable.Id));
                attributes.Add(idAttribute);
            }

            return attributes.Cast<ResourceFieldAttribute>().ToDictionary(key => key, value => (QueryLayer) null);
        }
    }
}
