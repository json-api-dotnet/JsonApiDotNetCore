using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Internal.Parsing;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.QueryStrings.Internal
{
    [PublicAPI]
    public class FilterQueryStringParameterReader : QueryStringParameterReader, IFilterQueryStringParameterReader
    {
        private static readonly LegacyFilterNotationConverter LegacyConverter = new();

        private readonly IJsonApiOptions _options;
        private readonly QueryStringParameterScopeParser _scopeParser;
        private readonly FilterParser _filterParser;
        private readonly ImmutableArray<FilterExpression>.Builder _filtersInGlobalScope = ImmutableArray.CreateBuilder<FilterExpression>();
        private readonly Dictionary<ResourceFieldChainExpression, ImmutableArray<FilterExpression>.Builder> _filtersPerScope = new();

        private string? _lastParameterName;

        public bool AllowEmptyValue => false;

        public FilterQueryStringParameterReader(IJsonApiRequest request, IResourceGraph resourceGraph, IResourceFactory resourceFactory,
            IJsonApiOptions options)
            : base(request, resourceGraph)
        {
            ArgumentGuard.NotNull(options, nameof(options));

            _options = options;
            _scopeParser = new QueryStringParameterScopeParser(FieldChainRequirements.EndsInToMany);
            _filterParser = new FilterParser(resourceFactory, ValidateSingleField);
        }

        protected void ValidateSingleField(ResourceFieldAttribute field, ResourceType resourceType, string path)
        {
            if (field is AttrAttribute attribute && !attribute.Capabilities.HasFlag(AttrCapabilities.AllowFilter))
            {
                throw new InvalidQueryStringParameterException(_lastParameterName!, "Filtering on the requested attribute is not allowed.",
                    $"Filtering on attribute '{attribute.PublicName}' is not allowed.");
            }
        }

        /// <inheritdoc />
        public virtual bool IsEnabled(DisableQueryStringAttribute disableQueryStringAttribute)
        {
            ArgumentGuard.NotNull(disableQueryStringAttribute, nameof(disableQueryStringAttribute));

            return !IsAtomicOperationsRequest && !disableQueryStringAttribute.ContainsParameter(JsonApiQueryStringParameters.Filter);
        }

        /// <inheritdoc />
        public virtual bool CanRead(string parameterName)
        {
            ArgumentGuard.NotNullNorEmpty(parameterName, nameof(parameterName));

            bool isNested = parameterName.StartsWith("filter[", StringComparison.Ordinal) && parameterName.EndsWith("]", StringComparison.Ordinal);
            return parameterName == "filter" || isNested;
        }

        /// <inheritdoc />
        public virtual void Read(string parameterName, StringValues parameterValue)
        {
            _lastParameterName = parameterName;

            foreach (string value in parameterValue.SelectMany(ExtractParameterValue))
            {
                ReadSingleValue(parameterName, value);
            }
        }

        private IEnumerable<string> ExtractParameterValue(string parameterValue)
        {
            if (_options.EnableLegacyFilterNotation)
            {
                foreach (string condition in LegacyConverter.ExtractConditions(parameterValue))
                {
                    yield return condition;
                }
            }
            else
            {
                yield return parameterValue;
            }
        }

        private void ReadSingleValue(string parameterName, string parameterValue)
        {
            try
            {
                string name = parameterName;
                string value = parameterValue;

                if (_options.EnableLegacyFilterNotation)
                {
                    (name, value) = LegacyConverter.Convert(name, value);
                }

                ResourceFieldChainExpression? scope = GetScope(name);
                FilterExpression filter = GetFilter(value, scope);

                StoreFilterInScope(filter, scope);
            }
            catch (QueryParseException exception)
            {
                throw new InvalidQueryStringParameterException(_lastParameterName!, "The specified filter is invalid.", exception.Message, exception);
            }
        }

        private ResourceFieldChainExpression? GetScope(string parameterName)
        {
            QueryStringParameterScopeExpression parameterScope = _scopeParser.Parse(parameterName, RequestResourceType);

            if (parameterScope.Scope == null)
            {
                AssertIsCollectionRequest();
            }

            return parameterScope.Scope;
        }

        private FilterExpression GetFilter(string parameterValue, ResourceFieldChainExpression? scope)
        {
            ResourceType resourceTypeInScope = GetResourceTypeForScope(scope);
            return _filterParser.Parse(parameterValue, resourceTypeInScope);
        }

        private void StoreFilterInScope(FilterExpression filter, ResourceFieldChainExpression? scope)
        {
            if (scope == null)
            {
                _filtersInGlobalScope.Add(filter);
            }
            else
            {
                if (!_filtersPerScope.ContainsKey(scope))
                {
                    _filtersPerScope[scope] = ImmutableArray.CreateBuilder<FilterExpression>();
                }

                _filtersPerScope[scope].Add(filter);
            }
        }

        /// <inheritdoc />
        public virtual IReadOnlyCollection<ExpressionInScope> GetConstraints()
        {
            return EnumerateFiltersInScopes().ToArray();
        }

        private IEnumerable<ExpressionInScope> EnumerateFiltersInScopes()
        {
            if (_filtersInGlobalScope.Any())
            {
                FilterExpression filter = MergeFilters(_filtersInGlobalScope.ToImmutable());
                yield return new ExpressionInScope(null, filter);
            }

            foreach ((ResourceFieldChainExpression scope, ImmutableArray<FilterExpression>.Builder filtersBuilder) in _filtersPerScope)
            {
                FilterExpression filter = MergeFilters(filtersBuilder.ToImmutable());
                yield return new ExpressionInScope(scope, filter);
            }
        }

        private static FilterExpression MergeFilters(IImmutableList<FilterExpression> filters)
        {
            return filters.Count > 1 ? new LogicalExpression(LogicalOperator.Or, filters) : filters.First();
        }
    }
}
