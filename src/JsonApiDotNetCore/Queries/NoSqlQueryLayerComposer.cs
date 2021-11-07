using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Internal;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

#pragma warning disable AV2310 // Code block should not contain inline comment

namespace JsonApiDotNetCore.Queries
{
    /// <summary>
    /// Default implementation of the <see cref="INoSqlQueryLayerComposer" />.
    /// </summary>
    /// <remarks>
    /// Register <see cref="NoSqlQueryLayerComposer"/> with the service container as
    /// shown in the following example.
    /// </remarks>
    /// <example>
    /// <code><![CDATA[
    /// public class Startup
    /// {
    ///     public void ConfigureServices(IServiceCollection services)
    ///     {
    ///         services.AddNoSqlResourceServices();
    ///     }
    /// }
    /// ]]></code>
    /// </example>
    [PublicAPI]
    public class NoSqlQueryLayerComposer : QueryLayerComposer, INoSqlQueryLayerComposer
    {
        private readonly IEnumerable<IQueryConstraintProvider> _constraintProviders;
        private readonly ITargetedFields _targetedFields;

        public NoSqlQueryLayerComposer(
            IEnumerable<IQueryConstraintProvider> constraintProviders,
            IResourceDefinitionAccessor resourceDefinitionAccessor,
            IJsonApiOptions options,
            IPaginationContext paginationContext,
            ITargetedFields targetedFields,
            IEvaluatedIncludeCache evaluatedIncludeCache,
            ISparseFieldSetCache sparseFieldSetCache)
            : base(constraintProviders, resourceDefinitionAccessor, options, paginationContext, targetedFields, evaluatedIncludeCache, sparseFieldSetCache)
        {
            _constraintProviders = constraintProviders;
            _targetedFields = targetedFields;
        }

        /// <inheritdoc />
        public FilterExpression? GetPrimaryFilterFromConstraintsForNoSql(ResourceType primaryResourceType)
        {
            return GetPrimaryFilterFromConstraints(primaryResourceType);
        }

        /// <inheritdoc />
        public (QueryLayer QueryLayer, IncludeExpression Include) ComposeFromConstraintsForNoSql(ResourceType requestResourceType)
        {
            QueryLayer queryLayer = ComposeFromConstraints(requestResourceType);
            IncludeExpression include = queryLayer.Include ?? IncludeExpression.Empty;

            queryLayer.Include = IncludeExpression.Empty;
            queryLayer.Projection = null;

            return (queryLayer, include);
        }

        /// <inheritdoc />
        public (QueryLayer QueryLayer, IncludeExpression Include) ComposeForGetByIdWithConstraintsForNoSql<TId>(
            TId id,
            ResourceType primaryResourceType,
            TopFieldSelection fieldSelection)
            where TId : notnull
        {
            QueryLayer queryLayer = ComposeForGetById(id, primaryResourceType, fieldSelection);
            IncludeExpression include = queryLayer.Include ?? IncludeExpression.Empty;

            queryLayer.Include = IncludeExpression.Empty;
            queryLayer.Projection = null;

            return (queryLayer, include);
        }

        /// <inheritdoc />
        public QueryLayer ComposeForGetByIdForNoSql<TId>(TId id, ResourceType primaryResourceType)
            where TId : notnull
        {
            return new QueryLayer(primaryResourceType)
            {
                Filter = new ComparisonExpression(ComparisonOperator.Equals,
                    new ResourceFieldChainExpression(primaryResourceType.Fields.Single(field => field.Property.Name == nameof(IIdentifiable<TId>.Id))),
                    new LiteralConstantExpression(id.ToString()!)),
                Include = IncludeExpression.Empty
            };
        }

        /// <inheritdoc />
        public (QueryLayer QueryLayer, IncludeExpression Include) ComposeFromConstraintsForNoSql(
            ResourceType requestResourceType,
            string propertyName,
            string propertyValue,
            bool isIncluded)
        {
            // Compose a secondary resource filter in the form "equals({propertyName},'{propertyValue}')".
            FilterExpression[] secondaryResourceFilterExpressions =
            {
                ComposeSecondaryResourceFilter(requestResourceType, propertyName, propertyValue)
            };

            // Get the query expressions from the request.
            ExpressionInScope[] constraints = _constraintProviders.SelectMany(provider => provider.GetConstraints()).ToArray();

            bool IsQueryLayerConstraint(ExpressionInScope constraint)
            {
                return constraint.Expression is not IncludeExpression && (!isIncluded || (constraint.Scope is not null &&
                    constraint.Scope.Fields.Any(field => field.PublicName == requestResourceType.PublicName)));
            }

            IEnumerable<QueryExpression> requestQueryExpressions = constraints.Where(IsQueryLayerConstraint).Select(constraint => constraint.Expression);

            // Combine the secondary resource filter and request query expressions and
            // create the query layer from the combined query expressions.
            QueryExpression[] queryExpressions = secondaryResourceFilterExpressions.Concat(requestQueryExpressions).ToArray();

            var queryLayer = new QueryLayer(requestResourceType)
            {
                Include = IncludeExpression.Empty,
                Filter = GetFilter(queryExpressions, requestResourceType),
                Sort = GetSort(queryExpressions, requestResourceType),
                Pagination = GetPagination(queryExpressions, requestResourceType)
            };

            // Retrieve the IncludeExpression from the constraints collection.
            // There will be zero or one IncludeExpression, even if multiple include query
            // parameters were specified in the request. JsonApiDotNetCore combines those
            // into a single expression.
            IncludeExpression include = isIncluded
                ? IncludeExpression.Empty
                : constraints.Select(constraint => constraint.Expression).OfType<IncludeExpression>().DefaultIfEmpty(IncludeExpression.Empty).Single();

            return (queryLayer, include);
        }

        private static FilterExpression ComposeSecondaryResourceFilter(ResourceType resourceType, string propertyName, string properyValue)
        {
            return new ComparisonExpression(ComparisonOperator.Equals,
                new ResourceFieldChainExpression(resourceType.Fields.Single(field => field.Property.Name == propertyName)),
                new LiteralConstantExpression(properyValue));
        }

        public (QueryLayer QueryLayer, IncludeExpression Include) ComposeForUpdateForNoSql<TId>(TId id, ResourceType primaryResourceType)
            where TId : notnull
        {
            // Create primary layer without an include expression.
            AttrAttribute primaryIdAttribute = GetIdAttribute(primaryResourceType);

            QueryLayer primaryLayer = new(primaryResourceType)
            {
                Include = IncludeExpression.Empty,
                Filter = new ComparisonExpression(ComparisonOperator.Equals, new ResourceFieldChainExpression(primaryIdAttribute),
                    new LiteralConstantExpression(id.ToString()!))
            };

            // Create a separate include expression.
            ImmutableHashSet<IncludeElementExpression> includeElements = _targetedFields.Relationships
                .Select(relationship => new IncludeElementExpression(relationship)).ToImmutableHashSet();

            IncludeExpression include = includeElements.Any() ? new IncludeExpression(includeElements) : IncludeExpression.Empty;

            return (primaryLayer, include);
        }

        private static AttrAttribute GetIdAttribute(ResourceType resourceType)
        {
            return resourceType.GetAttributeByPropertyName(nameof(Identifiable<object>.Id));
        }
    }
}
