using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.Archiving
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class TelevisionBroadcastDefinition : JsonApiResourceDefinition<TelevisionBroadcast, int>
    {
        private readonly TelevisionDbContext _dbContext;
        private readonly IJsonApiRequest _request;
        private readonly IEnumerable<IQueryConstraintProvider> _constraintProviders;

        private DateTimeOffset? _storedArchivedAt;

        public TelevisionBroadcastDefinition(IResourceGraph resourceGraph, TelevisionDbContext dbContext, IJsonApiRequest request,
            IEnumerable<IQueryConstraintProvider> constraintProviders)
            : base(resourceGraph)
        {
            _dbContext = dbContext;
            _request = request;
            _constraintProviders = constraintProviders;
        }

        public override FilterExpression? OnApplyFilter(FilterExpression? existingFilter)
        {
            if (_request.IsReadOnly)
            {
                // Rule: hide archived broadcasts in collections, unless a filter is specified.

                if (IsReturningCollectionOfTelevisionBroadcasts() && !HasFilterOnArchivedAt(existingFilter))
                {
                    AttrAttribute archivedAtAttribute = ResourceType.GetAttributeByPropertyName(nameof(TelevisionBroadcast.ArchivedAt));
                    var archivedAtChain = new ResourceFieldChainExpression(archivedAtAttribute);

                    FilterExpression isUnarchived = new ComparisonExpression(ComparisonOperator.Equals, archivedAtChain, NullConstantExpression.Instance);

                    return LogicalExpression.Compose(LogicalOperator.And, existingFilter, isUnarchived);
                }
            }

            return existingFilter;
        }

        private bool IsReturningCollectionOfTelevisionBroadcasts()
        {
            return IsRequestingCollectionOfTelevisionBroadcasts() || IsIncludingCollectionOfTelevisionBroadcasts();
        }

        private bool IsRequestingCollectionOfTelevisionBroadcasts()
        {
            if (_request.IsCollection)
            {
                if (ResourceType.Equals(_request.PrimaryResourceType) || ResourceType.Equals(_request.SecondaryResourceType))
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
                if (includeElement.Relationship is HasManyAttribute && includeElement.Relationship.RightType.Equals(ResourceType))
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasFilterOnArchivedAt(FilterExpression? existingFilter)
        {
            if (existingFilter == null)
            {
                return false;
            }

            var walker = new FilterWalker();
            walker.Visit(existingFilter, null);

            return walker.HasFilterOnArchivedAt;
        }

        public override Task OnPrepareWriteAsync(TelevisionBroadcast broadcast, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        {
            if (writeOperation == WriteOperationKind.UpdateResource)
            {
                _storedArchivedAt = broadcast.ArchivedAt;
            }

            return Task.CompletedTask;
        }

        public override async Task OnWritingAsync(TelevisionBroadcast broadcast, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        {
            if (writeOperation == WriteOperationKind.CreateResource)
            {
                AssertIsNotArchived(broadcast);
            }
            else if (writeOperation == WriteOperationKind.UpdateResource)
            {
                AssertIsNotShiftingArchiveDate(broadcast);
            }
            else if (writeOperation == WriteOperationKind.DeleteResource)
            {
                TelevisionBroadcast? broadcastToDelete = await _dbContext.Broadcasts.FirstWithIdAsync(broadcast.Id, cancellationToken);

                if (broadcastToDelete != null)
                {
                    AssertIsArchived(broadcastToDelete);
                }
            }

            await base.OnWritingAsync(broadcast, writeOperation, cancellationToken);
        }

        [AssertionMethod]
        private static void AssertIsNotArchived(TelevisionBroadcast broadcast)
        {
            if (broadcast.ArchivedAt != null)
            {
                throw new JsonApiException(new ErrorObject(HttpStatusCode.Forbidden)
                {
                    Title = "Television broadcasts cannot be created in archived state."
                });
            }
        }

        [AssertionMethod]
        private void AssertIsNotShiftingArchiveDate(TelevisionBroadcast broadcast)
        {
            if (_storedArchivedAt != null && broadcast.ArchivedAt != null && _storedArchivedAt != broadcast.ArchivedAt)
            {
                throw new JsonApiException(new ErrorObject(HttpStatusCode.Forbidden)
                {
                    Title = "Archive date of television broadcasts cannot be shifted. Unarchive it first."
                });
            }
        }

        [AssertionMethod]
        private static void AssertIsArchived(TelevisionBroadcast broadcast)
        {
            if (broadcast.ArchivedAt == null)
            {
                throw new JsonApiException(new ErrorObject(HttpStatusCode.Forbidden)
                {
                    Title = "Television broadcasts must first be archived before they can be deleted."
                });
            }
        }

        private sealed class FilterWalker : QueryExpressionRewriter<object?>
        {
            public bool HasFilterOnArchivedAt { get; private set; }

            public override QueryExpression? VisitResourceFieldChain(ResourceFieldChainExpression expression, object? argument)
            {
                if (expression.Fields[0].Property.Name == nameof(TelevisionBroadcast.ArchivedAt))
                {
                    HasFilterOnArchivedAt = true;
                }

                return base.VisitResourceFieldChain(expression, argument);
            }
        }
    }
}
