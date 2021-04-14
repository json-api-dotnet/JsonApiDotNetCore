using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Archiving
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class TelevisionBroadcastDefinition : JsonApiResourceDefinition<TelevisionBroadcast>
    {
        private readonly TelevisionDbContext _dbContext;
        private readonly IJsonApiRequest _request;
        private readonly IEnumerable<IQueryConstraintProvider> _constraintProviders;
        private readonly ResourceContext _broadcastContext;

        private DateTimeOffset? _storedArchivedAt;

        public TelevisionBroadcastDefinition(IResourceGraph resourceGraph, TelevisionDbContext dbContext, IJsonApiRequest request,
            IEnumerable<IQueryConstraintProvider> constraintProviders)
            : base(resourceGraph)
        {
            _dbContext = dbContext;
            _request = request;
            _constraintProviders = constraintProviders;
            _broadcastContext = resourceGraph.GetResourceContext<TelevisionBroadcast>();
        }

        public override FilterExpression OnApplyFilter(FilterExpression existingFilter)
        {
            if (_request.IsReadOnly)
            {
                // Rule: hide archived broadcasts in collections, unless a filter is specified.

                if (IsReturningCollectionOfTelevisionBroadcasts() && !HasFilterOnArchivedAt(existingFilter))
                {
                    AttrAttribute archivedAtAttribute =
                        _broadcastContext.Attributes.Single(attr => attr.Property.Name == nameof(TelevisionBroadcast.ArchivedAt));

                    var archivedAtChain = new ResourceFieldChainExpression(archivedAtAttribute);

                    FilterExpression isUnarchived = new ComparisonExpression(ComparisonOperator.Equals, archivedAtChain, new NullConstantExpression());

                    return existingFilter == null
                        ? isUnarchived
                        : new LogicalExpression(LogicalOperator.And, ArrayFactory.Create(existingFilter, isUnarchived));
                }
            }

            return base.OnApplyFilter(existingFilter);
        }

        private bool IsReturningCollectionOfTelevisionBroadcasts()
        {
            return IsRequestingCollectionOfTelevisionBroadcasts() || IsIncludingCollectionOfTelevisionBroadcasts();
        }

        private bool IsRequestingCollectionOfTelevisionBroadcasts()
        {
            if (_request.IsCollection)
            {
                if (_request.PrimaryResource == _broadcastContext || _request.SecondaryResource == _broadcastContext)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsIncludingCollectionOfTelevisionBroadcasts()
        {
            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:keep_existing_linebreaks true

            IncludeElementExpression[] includeElements = _constraintProviders
                .SelectMany(provider => provider.GetConstraints())
                .Select(expressionInScope => expressionInScope.Expression)
                .OfType<IncludeExpression>()
                .SelectMany(include => include.Elements)
                .ToArray();

            // @formatter:keep_existing_linebreaks restore
            // @formatter:wrap_chained_method_calls restore

            foreach (IncludeElementExpression includeElement in includeElements)
            {
                if (includeElement.Relationship is HasManyAttribute && includeElement.Relationship.RightType == _broadcastContext.ResourceType)
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasFilterOnArchivedAt(FilterExpression existingFilter)
        {
            if (existingFilter == null)
            {
                return false;
            }

            var walker = new FilterWalker();
            walker.Visit(existingFilter, null);

            return walker.HasFilterOnArchivedAt;
        }

        public override Task OnBeforeCreateResourceAsync(TelevisionBroadcast broadcast, CancellationToken cancellationToken)
        {
            AssertIsNotArchived(broadcast);

            return base.OnBeforeCreateResourceAsync(broadcast, cancellationToken);
        }

        [AssertionMethod]
        private static void AssertIsNotArchived(TelevisionBroadcast broadcast)
        {
            if (broadcast.ArchivedAt != null)
            {
                throw new JsonApiException(new Error(HttpStatusCode.Forbidden)
                {
                    Title = "Television broadcasts cannot be created in archived state."
                });
            }
        }

        public override Task OnAfterGetForUpdateResourceAsync(TelevisionBroadcast resource, CancellationToken cancellationToken)
        {
            _storedArchivedAt = resource.ArchivedAt;

            return base.OnAfterGetForUpdateResourceAsync(resource, cancellationToken);
        }

        public override Task OnBeforeUpdateResourceAsync(TelevisionBroadcast resource, CancellationToken cancellationToken)
        {
            AssertIsNotShiftingArchiveDate(resource);

            return base.OnBeforeUpdateResourceAsync(resource, cancellationToken);
        }

        [AssertionMethod]
        private void AssertIsNotShiftingArchiveDate(TelevisionBroadcast resource)
        {
            if (_storedArchivedAt != null && resource.ArchivedAt != null && _storedArchivedAt != resource.ArchivedAt)
            {
                throw new JsonApiException(new Error(HttpStatusCode.Forbidden)
                {
                    Title = "Archive date of television broadcasts cannot be shifted. Unarchive it first."
                });
            }
        }

        public override async Task OnBeforeDeleteResourceAsync(int broadcastId, CancellationToken cancellationToken)
        {
            TelevisionBroadcast televisionBroadcast =
                await _dbContext.Broadcasts.FirstOrDefaultAsync(broadcast => broadcast.Id == broadcastId, cancellationToken);

            if (televisionBroadcast != null)
            {
                AssertIsArchived(televisionBroadcast);
            }

            await base.OnBeforeDeleteResourceAsync(broadcastId, cancellationToken);
        }

        [AssertionMethod]
        private static void AssertIsArchived(TelevisionBroadcast televisionBroadcast)
        {
            if (televisionBroadcast.ArchivedAt == null)
            {
                throw new JsonApiException(new Error(HttpStatusCode.Forbidden)
                {
                    Title = "Television broadcasts must first be archived before they can be deleted."
                });
            }
        }

        private sealed class FilterWalker : QueryExpressionRewriter<object>
        {
            public bool HasFilterOnArchivedAt { get; private set; }

            public override QueryExpression VisitResourceFieldChain(ResourceFieldChainExpression expression, object argument)
            {
                if (expression.Fields.First().Property.Name == nameof(TelevisionBroadcast.ArchivedAt))
                {
                    HasFilterOnArchivedAt = true;
                }

                return base.VisitResourceFieldChain(expression, argument);
            }
        }
    }
}
