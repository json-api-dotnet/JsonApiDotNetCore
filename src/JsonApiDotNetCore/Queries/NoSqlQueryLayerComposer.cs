using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Internal;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

#pragma warning disable AV1551 // Method overload should call another overload
#pragma warning disable AV2310 // Code block should not contain inline comment

namespace JsonApiDotNetCore.Queries
{
    /// <summary>
    /// Default implementation of the <see cref="INoSqlQueryLayerComposer" />.
    /// </summary>
    /// <remarks>
    /// Register <see cref="NoSqlQueryLayerComposer" /> with the service container as shown in the following example.
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

        // ReSharper disable PossibleMultipleEnumeration

        public NoSqlQueryLayerComposer(IEnumerable<IQueryConstraintProvider> constraintProviders, IResourceDefinitionAccessor resourceDefinitionAccessor,
            IJsonApiOptions options, IPaginationContext paginationContext, ITargetedFields targetedFields, IEvaluatedIncludeCache evaluatedIncludeCache,
            ISparseFieldSetCache sparseFieldSetCache)
            : base(constraintProviders, resourceDefinitionAccessor, options, paginationContext, targetedFields, evaluatedIncludeCache, sparseFieldSetCache)
        {
            _constraintProviders = constraintProviders;
            _targetedFields = targetedFields;
        }

        // ReSharper restore PossibleMultipleEnumeration

        /// <inheritdoc />
        public FilterExpression? GetPrimaryFilterFromConstraintsForNoSql(ResourceType primaryResourceType)
        {
            return AssertFilterExpressionIsSimple(GetPrimaryFilterFromConstraints(primaryResourceType));
        }

        /// <inheritdoc />
        public (QueryLayer QueryLayer, IncludeExpression Include) ComposeFromConstraintsForNoSql(ResourceType requestResourceType)
        {
            QueryLayer queryLayer = ComposeFromConstraints(requestResourceType);

            IncludeExpression include = AssertIncludeExpressionIsSimple(queryLayer.Include);

            queryLayer.Filter = AssertFilterExpressionIsSimple(queryLayer.Filter);
            queryLayer.Include = IncludeExpression.Empty;
            queryLayer.Projection = null;

            return (queryLayer, include);
        }

        /// <inheritdoc />
        public (QueryLayer QueryLayer, IncludeExpression Include) ComposeFromConstraintsForNoSql(ResourceType requestResourceType, string propertyName,
            string propertyValue, bool isIncluded)
        {
            // Compose a secondary resource filter in the form "equals({propertyName},'{propertyValue}')".
            FilterExpression[] secondaryResourceFilterExpressions =
            {
                ComposeSecondaryResourceFilter(requestResourceType, propertyName, propertyValue)
            };

            // @formatter:off

            // Get the query expressions from the request.
            ExpressionInScope[] constraints = _constraintProviders
                .SelectMany(provider => provider.GetConstraints())
                .ToArray();

            bool IsQueryLayerConstraint(ExpressionInScope constraint)
            {
                return constraint.Expression is not IncludeExpression && (!isIncluded || IsResourceScoped(constraint));
            }

            bool IsResourceScoped(ExpressionInScope constraint)
            {
                return constraint.Scope is not null &&
                       constraint.Scope.Fields.Any(field => field.PublicName == requestResourceType.PublicName);
            }

            QueryExpression[] requestQueryExpressions = constraints
                .Where(IsQueryLayerConstraint)
                .Select(constraint => constraint.Expression)
                .ToArray();

            FilterExpression[] requestFilterExpressions = requestQueryExpressions
                .OfType<FilterExpression>()
                .Select(filterExpression => AssertFilterExpressionIsSimple(filterExpression)!)
                .ToArray();

            FilterExpression[] combinedFilterExpressions = secondaryResourceFilterExpressions
                .Concat(requestFilterExpressions)
                .ToArray();

            var queryLayer = new QueryLayer(requestResourceType)
            {
                Include = IncludeExpression.Empty,
                Filter = GetFilter(combinedFilterExpressions, requestResourceType),
                Sort = GetSort(requestQueryExpressions, requestResourceType),
                Pagination = GetPagination(requestQueryExpressions, requestResourceType)
            };

            // Retrieve the IncludeExpression from the constraints collection.
            // There will be zero or one IncludeExpression, even if multiple include query
            // parameters were specified in the request. JsonApiDotNetCore combines those
            // into a single expression.
            IncludeExpression include = isIncluded
                ? IncludeExpression.Empty
                : AssertIncludeExpressionIsSimple(constraints
                    .Select(constraint => constraint.Expression)
                    .OfType<IncludeExpression>()
                    .DefaultIfEmpty(IncludeExpression.Empty)
                    .Single());

            // @formatter:on

            return (queryLayer, include);
        }

        private static FilterExpression ComposeSecondaryResourceFilter(ResourceType resourceType, string propertyName, string properyValue)
        {
            return new ComparisonExpression(ComparisonOperator.Equals,
                new ResourceFieldChainExpression(resourceType.Fields.Single(field => field.Property.Name == propertyName)),
                new LiteralConstantExpression(properyValue));
        }

        /// <inheritdoc />
        public (QueryLayer QueryLayer, IncludeExpression Include) ComposeForGetByIdWithConstraintsForNoSql<TId>(TId id, ResourceType primaryResourceType,
            TopFieldSelection fieldSelection)
            where TId : notnull
        {
            QueryLayer queryLayer = ComposeForGetById(id, primaryResourceType, fieldSelection);

            IncludeExpression include = AssertIncludeExpressionIsSimple(queryLayer.Include);

            queryLayer.Filter = AssertFilterExpressionIsSimple(queryLayer.Filter);
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

        private static FilterExpression? AssertFilterExpressionIsSimple(FilterExpression? filterExpression)
        {
            if (filterExpression is null)
            {
                return filterExpression;
            }

            var visitor = new FilterExpressionVisitor();

            return visitor.Visit(filterExpression, null)
                ? filterExpression
                : throw new JsonApiException(new ErrorObject(HttpStatusCode.BadRequest)
                {
                    Title = "Unsupported filter expression",
                    Detail = "Navigation of to-one or to-many relationships is not supported."
                });
        }

        private static IncludeExpression AssertIncludeExpressionIsSimple(IncludeExpression? includeExpression)
        {
            if (includeExpression is null)
            {
                return IncludeExpression.Empty;
            }

            return includeExpression.Elements.Any(element => element.Children.Any())
                ? throw new JsonApiException(new ErrorObject(HttpStatusCode.BadRequest)
                {
                    Title = "Unsupported include expression",
                    Detail = "Multi-level include expressions are not supported."
                })
                : includeExpression;
        }

        private sealed class FilterExpressionVisitor : QueryExpressionVisitor<object?, bool>
        {
            private bool _isSimpleFilterExpression = true;

            /// <inheritdoc />
            public override bool DefaultVisit(QueryExpression expression, object? argument)
            {
                return _isSimpleFilterExpression;
            }

            /// <inheritdoc />
            public override bool VisitComparison(ComparisonExpression expression, object? argument)
            {
                return expression.Left.Accept(this, argument) && expression.Right.Accept(this, argument);
            }

            /// <inheritdoc />
            public override bool VisitResourceFieldChain(ResourceFieldChainExpression expression, object? argument)
            {
                _isSimpleFilterExpression &= expression.Fields.All(IsFieldSupported);

                return _isSimpleFilterExpression;
            }

            private static bool IsFieldSupported(ResourceFieldAttribute field)
            {
                return field switch
                {
                    AttrAttribute => true,
                    HasManyAttribute hasMany when HasOwnsManyAttribute(hasMany) => true,
                    _ => false
                };
            }

            private static bool HasOwnsManyAttribute(ResourceFieldAttribute field)
            {
                return Attribute.GetCustomAttribute(field.Property, typeof(NoSqlOwnsManyAttribute)) is not null;
            }

            /// <inheritdoc />
            public override bool VisitLogical(LogicalExpression expression, object? argument)
            {
                return expression.Terms.All(term => term.Accept(this, argument));
            }

            /// <inheritdoc />
            public override bool VisitNot(NotExpression expression, object? argument)
            {
                return expression.Child.Accept(this, argument);
            }

            /// <inheritdoc />
            public override bool VisitHas(HasExpression expression, object? argument)
            {
                return expression.TargetCollection.Accept(this, argument) && (expression.Filter is null || expression.Filter.Accept(this, argument));
            }

            /// <inheritdoc />
            public override bool VisitSortElement(SortElementExpression expression, object? argument)
            {
                return expression.TargetAttribute is null || expression.TargetAttribute.Accept(this, argument);
            }

            /// <inheritdoc />
            public override bool VisitSort(SortExpression expression, object? argument)
            {
                return expression.Elements.All(element => element.Accept(this, argument));
            }

            /// <inheritdoc />
            public override bool VisitCount(CountExpression expression, object? argument)
            {
                return expression.TargetCollection.Accept(this, argument);
            }

            /// <inheritdoc />
            public override bool VisitMatchText(MatchTextExpression expression, object? argument)
            {
                return expression.TargetAttribute.Accept(this, argument);
            }

            /// <inheritdoc />
            public override bool VisitAny(AnyExpression expression, object? argument)
            {
                return expression.TargetAttribute.Accept(this, argument);
            }
        }
    }
}
