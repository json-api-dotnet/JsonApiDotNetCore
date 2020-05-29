using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Internal.Queries.Expressions;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Annotation;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceDefinitions
{
    public sealed class CallableResourceDefinition : ResourceDefinition<CallableResource>
    {
        private static readonly PageSize _maxPageSize = new PageSize(5);

        public CallableResourceDefinition(IResourceGraph resourceGraph) : base(resourceGraph)
        {
            // This constructor will be resolved from the container, which means
            // you can take on any dependency that is also defined in the container.
        }

        public override FilterExpression OnApplyFilter(FilterExpression existingFilter)
        {
            // Use case: automatically exclude deleted resources for all requests.

            var resourceContext = ResourceGraph.GetResourceContext<CallableResource>();
            var isDeletedAttribute = resourceContext.Attributes.Single(a => a.Property.Name == nameof(CallableResource.IsDeleted));

            var isNotDeleted = new ComparisonExpression(ComparisonOperator.Equals,
                new ResourceFieldChainExpression(isDeletedAttribute), new LiteralConstantExpression(bool.FalseString));

            return existingFilter == null
                ? (FilterExpression) isNotDeleted
                : new LogicalExpression(LogicalOperator.And, new[] {isNotDeleted, existingFilter});
        }

        public override SortExpression OnApplySort(SortExpression existingSort)
        {
            // Use case: set a default sort order when none was specified in query string.

            if (existingSort != null)
            {
                return existingSort;
            }
            
            return CreateSortExpressionFromLambda(new PropertySortOrder
            {
                (resource => resource.Label, ListSortDirection.Ascending),
                (resource => resource.ModifiedAt, ListSortDirection.Descending)
            });
        }

        public override PaginationExpression OnApplyPagination(PaginationExpression existingPagination)
        {
            // Use case: enforce a page size of 5 or less for this resource type.

            if (existingPagination != null)
            {
                var pageSize = existingPagination.PageSize?.Value <= _maxPageSize.Value ? existingPagination.PageSize : _maxPageSize;
                return new PaginationExpression(existingPagination.PageNumber, pageSize);
            }

            return new PaginationExpression(PageNumber.ValueOne, _maxPageSize);
        }

        public override SparseFieldSetExpression OnApplySparseFieldSet(SparseFieldSetExpression existingSparseFieldSet)
        {
            // Use case: always include percentageComplete and never include riskLevel in responses.

            return existingSparseFieldSet
                .Including<CallableResource>(resource => resource.PercentageComplete, ResourceGraph)
                .Excluding<CallableResource>(resource => resource.RiskLevel, ResourceGraph);
        }

        protected override QueryStringParameterHandlers OnRegisterQueryableHandlersForQueryStringParameters()
        {
            // Use case: 'isHighRisk' query string parameter can be used to add extra filter on IQueryable<TResource>.

            return new QueryStringParameterHandlers
            {
                ["isHighRisk"] = FilterByHighRisk
            };
        }

        private static IQueryable<CallableResource> FilterByHighRisk(IQueryable<CallableResource> source, StringValues parameterValue)
        {
            bool isFilterOnHighRisk = bool.Parse(parameterValue);
            return isFilterOnHighRisk ? source.Where(resource => resource.RiskLevel >= 5) : source.Where(resource => resource.RiskLevel < 5);
        }
    }
}
