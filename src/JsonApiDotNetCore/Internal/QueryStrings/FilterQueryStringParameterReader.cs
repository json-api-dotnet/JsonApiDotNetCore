using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Exceptions;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Internal.Queries;
using JsonApiDotNetCore.Internal.Queries.Expressions;
using JsonApiDotNetCore.Internal.Queries.Parsing;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Annotation;
using JsonApiDotNetCore.RequestServices.Contracts;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.Internal.QueryStrings
{
    /// <summary>
    /// Reads the 'filter' query string parameter and produces a set of query constraints from it.
    /// </summary>
    public interface IFilterQueryStringParameterReader : IQueryStringParameterReader, IQueryConstraintProvider
    {
    }

    public class FilterQueryStringParameterReader : QueryStringParameterReader, IFilterQueryStringParameterReader
    {
        private static readonly LegacyFilterNotationConverter _legacyConverter = new LegacyFilterNotationConverter();

        private readonly IResourceFactory _resourceFactory;
        private readonly IJsonApiOptions _options;
        private readonly List<FilterExpression> _filtersInGlobalScope = new List<FilterExpression>();
        private readonly Dictionary<ResourceFieldChainExpression, List<FilterExpression>> _filtersPerScope = new Dictionary<ResourceFieldChainExpression, List<FilterExpression>>();
        private string _lastParameterName;

        public FilterQueryStringParameterReader(ICurrentRequest currentRequest,
            IResourceContextProvider resourceContextProvider, IResourceFactory resourceFactory, IJsonApiOptions options)
            : base(currentRequest, resourceContextProvider)
        {
            _resourceFactory = resourceFactory;
            _options = options;
        }

        public bool IsEnabled(DisableQueryAttribute disableQueryAttribute)
        {
            return !disableQueryAttribute.ContainsParameter(StandardQueryStringParameters.Filter);
        }

        public bool CanRead(string parameterName)
        {
            var isNested = parameterName.StartsWith("filter[") && parameterName.EndsWith("]");
            return parameterName == "filter" || isNested;
        }

        public void Read(string parameterName, StringValues parameterValues)
        {
            _lastParameterName = parameterName;

            foreach (string parameterValue in parameterValues)
            {
                ReadSingleValue(parameterName, parameterValue);
            }
        }

        private void ReadSingleValue(string parameterName, string parameterValue)
        {
            try
            {
                if (_options.EnableLegacyFilterNotation)
                {
                    (parameterName, parameterValue) = _legacyConverter.Convert(parameterName, parameterValue);
                }

                ResourceFieldChainExpression scope = GetScope(parameterName);
                FilterExpression filter = GetFilter(parameterValue, scope);

                StoreFilterInScope(filter, scope);
            }
            catch (QueryParseException exception)
            {
                throw new InvalidQueryStringParameterException(_lastParameterName, "The specified filter is invalid.", exception.Message, exception);
            }
        }

        private ResourceFieldChainExpression GetScope(string parameterName)
        {
            var parser = new QueryStringParameterScopeParser(parameterName,
                (path, _) => ChainResolver.ResolveToManyChain(RequestResource, path));

            var parameterScope = parser.Parse(FieldChainRequirements.EndsInToMany);

            if (parameterScope.Scope == null)
            {
                AssertIsCollectionRequest();
            }

            return parameterScope.Scope;
        }

        private FilterExpression GetFilter(string parameterValue, ResourceFieldChainExpression scope)
        {
            ResourceContext resourceContextInScope = GetResourceContextForScope(scope);

            var parser = new FilterParser(parameterValue,
                (path, chainRequirements) => ResolveChainInFilter(chainRequirements, resourceContextInScope, path),
                (resourceType, stringId) => TypeHelper.ConvertStringIdToTypedId(resourceType, stringId, _resourceFactory).ToString());

            return parser.Parse();
        }

        private IReadOnlyCollection<ResourceFieldAttribute> ResolveChainInFilter(FieldChainRequirements chainRequirements,
            ResourceContext resourceContextInScope, string path)
        {
            if (chainRequirements == FieldChainRequirements.EndsInToMany)
            {
                return ChainResolver.ResolveToOneChainEndingInToMany(resourceContextInScope, path);
            }

            if (chainRequirements == FieldChainRequirements.EndsInAttribute)
            {
                return ChainResolver.ResolveToOneChainEndingInAttribute(resourceContextInScope, path, ValidateFilter);
            }

            if (chainRequirements.HasFlag(FieldChainRequirements.EndsInAttribute) &&
                chainRequirements.HasFlag(FieldChainRequirements.EndsInToOne))
            {
                return ChainResolver.ResolveToOneChainEndingInAttributeOrToOne(resourceContextInScope, path, ValidateFilter);
            }

            throw new InvalidOperationException($"Unexpected combination of chain requirement flags '{chainRequirements}'.");
        }

        private void ValidateFilter(ResourceFieldAttribute field, ResourceContext resourceContext, string path)
        {
            if (field is AttrAttribute attribute && !attribute.Capabilities.HasFlag(AttrCapabilities.AllowFilter))
            {
                throw new InvalidQueryStringParameterException(_lastParameterName, "Filtering on the requested attribute is not allowed.",
                    $"Filtering on attribute '{attribute.PublicName}' is not allowed.");
            }
        }

        private void StoreFilterInScope(FilterExpression filter, ResourceFieldChainExpression scope)
        {
            if (scope == null)
            {
                _filtersInGlobalScope.Add(filter);
            }
            else
            {
                if (!_filtersPerScope.ContainsKey(scope))
                {
                    _filtersPerScope[scope] = new List<FilterExpression>();
                }

                _filtersPerScope[scope].Add(filter);
            }
        }

        public IReadOnlyCollection<ExpressionInScope> GetConstraints()
        {
            return EnumerateFiltersInScopes().ToList().AsReadOnly();
        }

        private IEnumerable<ExpressionInScope> EnumerateFiltersInScopes()
        {
            if (_filtersInGlobalScope.Any())
            {
                var filter = MergeFilters(_filtersInGlobalScope);
                yield return new ExpressionInScope(null, filter);
            }

            foreach (var (scope, filters) in _filtersPerScope)
            {
                var filter = MergeFilters(filters);
                yield return new ExpressionInScope(scope, filter);
            }
        }

        private static FilterExpression MergeFilters(IReadOnlyCollection<FilterExpression> filters)
        {
            return filters.Count > 1 ? new LogicalExpression(LogicalOperator.Or, filters) : filters.First();
        }
    }
}
