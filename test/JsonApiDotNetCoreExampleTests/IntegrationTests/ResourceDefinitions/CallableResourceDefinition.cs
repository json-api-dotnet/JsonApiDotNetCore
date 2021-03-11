using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using JetBrains.Annotations;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceDefinitions
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class CallableResourceDefinition : JsonApiResourceDefinition<CallableResource>
    {
        private static readonly PageSize MaxPageSize = new PageSize(5);
        private readonly IUserRolesService _userRolesService;

        public CallableResourceDefinition(IResourceGraph resourceGraph, IUserRolesService userRolesService)
            : base(resourceGraph)
        {
            // This constructor will be resolved from the container, which means
            // you can take on any dependency that is also defined in the container.

            _userRolesService = userRolesService;
        }

        public override IReadOnlyCollection<IncludeElementExpression> OnApplyIncludes(IReadOnlyCollection<IncludeElementExpression> existingIncludes)
        {
            // Use case: prevent including owner if user has insufficient permissions.

            if (!_userRolesService.AllowIncludeOwner && existingIncludes.Any(include => include.Relationship.Property.Name == nameof(CallableResource.Owner)))
            {
                throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                {
                    Title = "Including owner is not permitted."
                });
            }

            return existingIncludes;
        }

        public override FilterExpression OnApplyFilter(FilterExpression existingFilter)
        {
            // Use case: automatically exclude deleted resources for all requests.

            ResourceContext resourceContext = ResourceGraph.GetResourceContext<CallableResource>();
            AttrAttribute isDeletedAttribute = resourceContext.Attributes.Single(attribute => attribute.Property.Name == nameof(CallableResource.IsDeleted));

            var isNotDeleted = new ComparisonExpression(ComparisonOperator.Equals, new ResourceFieldChainExpression(isDeletedAttribute),
                new LiteralConstantExpression(bool.FalseString));

            return existingFilter == null
                ? (FilterExpression)isNotDeleted
                : new LogicalExpression(LogicalOperator.And, ArrayFactory.Create(isNotDeleted, existingFilter));
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
                PageSize pageSize = existingPagination.PageSize?.Value <= MaxPageSize.Value ? existingPagination.PageSize : MaxPageSize;
                return new PaginationExpression(existingPagination.PageNumber, pageSize);
            }

            return new PaginationExpression(PageNumber.ValueOne, MaxPageSize);
        }

        public override SparseFieldSetExpression OnApplySparseFieldSet(SparseFieldSetExpression existingSparseFieldSet)
        {
            // Use case: always retrieve percentageComplete and never include riskLevel in responses.

            // @formatter:keep_existing_linebreaks true

            return existingSparseFieldSet
                .Including<CallableResource>(resource => resource.PercentageComplete, ResourceGraph)
                .Excluding<CallableResource>(resource => resource.RiskLevel, ResourceGraph);

            // @formatter:keep_existing_linebreaks restore
        }

        public override QueryStringParameterHandlers<CallableResource> OnRegisterQueryableHandlersForQueryStringParameters()
        {
            // Use case: 'isHighRisk' query string parameter can be used to add extra filter on IQueryable<TResource>.

            return new QueryStringParameterHandlers<CallableResource>
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
