using System.Collections.Immutable;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Parsing;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.QueryStrings;

/// <inheritdoc cref="IFilterQueryStringParameterReader" />
[PublicAPI]
public class FilterQueryStringParameterReader : QueryStringParameterReader, IFilterQueryStringParameterReader
{
    private static readonly LegacyFilterNotationConverter LegacyConverter = new();

    private readonly IJsonApiOptions _options;
    private readonly IQueryStringParameterScopeParser _scopeParser;
    private readonly IFilterParser _filterParser;
    private readonly ImmutableArray<FilterExpression>.Builder _filtersInGlobalScope = ImmutableArray.CreateBuilder<FilterExpression>();
    private readonly Dictionary<ResourceFieldChainExpression, ImmutableArray<FilterExpression>.Builder> _filtersPerScope = [];

    public bool AllowEmptyValue => false;

    public FilterQueryStringParameterReader(IQueryStringParameterScopeParser scopeParser, IFilterParser filterParser, IJsonApiRequest request,
        IResourceGraph resourceGraph, IJsonApiOptions options)
        : base(request, resourceGraph)
    {
        ArgumentNullException.ThrowIfNull(scopeParser);
        ArgumentNullException.ThrowIfNull(filterParser);
        ArgumentNullException.ThrowIfNull(options);

        _options = options;
        _scopeParser = scopeParser;
        _filterParser = filterParser;
    }

    /// <inheritdoc />
    public virtual bool IsEnabled(DisableQueryStringAttribute disableQueryStringAttribute)
    {
        ArgumentNullException.ThrowIfNull(disableQueryStringAttribute);

        return !IsAtomicOperationsRequest && !disableQueryStringAttribute.ContainsParameter(JsonApiQueryStringParameters.Filter);
    }

    /// <inheritdoc />
    public virtual bool CanRead(string parameterName)
    {
        ArgumentException.ThrowIfNullOrEmpty(parameterName);

        bool isNested = parameterName.StartsWith("filter[", StringComparison.Ordinal) && parameterName.EndsWith(']');
        return parameterName == "filter" || isNested;
    }

    /// <inheritdoc />
    public virtual void Read(string parameterName, StringValues parameterValue)
    {
        foreach (string value in parameterValue.SelectMany(ExtractParameterValue))
        {
            ReadSingleValue(parameterName, value);
        }
    }

    private IEnumerable<string> ExtractParameterValue(string? parameterValue)
    {
        if (parameterValue != null)
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
    }

    private void ReadSingleValue(string parameterName, string parameterValue)
    {
        bool parameterNameIsValid = false;

        try
        {
            string name = parameterName;
            string value = parameterValue;

            if (_options.EnableLegacyFilterNotation)
            {
                (name, value) = LegacyConverter.Convert(name, value);
            }

            IncludeExpression? scopeInclude = GetScope(name);
            parameterNameIsValid = true;

            foreach (ResourceFieldChainExpression? scopeChain in scopeInclude == null
                ? FieldChainInGlobalScope
                : IncludeChainConverter.Instance.GetRelationshipChains(scopeInclude))
            {
                FilterExpression filter = GetFilter(value, scopeChain);
                StoreFilterInScope(filter, scopeChain);
            }
        }
        catch (QueryParseException exception)
        {
            string specificMessage = _options.EnableLegacyFilterNotation
                ? exception.Message
                : exception.GetMessageWithPosition(parameterNameIsValid ? parameterValue : parameterName);

            throw new InvalidQueryStringParameterException(parameterName, "The specified filter is invalid.", specificMessage, exception);
        }
    }

    private IncludeExpression? GetScope(string parameterName)
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
            if (!_filtersPerScope.TryGetValue(scope, out ImmutableArray<FilterExpression>.Builder? builder))
            {
                builder = ImmutableArray.CreateBuilder<FilterExpression>();
                _filtersPerScope[scope] = builder;
            }

            builder.Add(filter);
        }
    }

    /// <inheritdoc />
    public virtual IReadOnlyCollection<ExpressionInScope> GetConstraints()
    {
        return EnumerateFiltersInScopes().ToArray().AsReadOnly();
    }

    private IEnumerable<ExpressionInScope> EnumerateFiltersInScopes()
    {
        if (_filtersInGlobalScope.Count > 0)
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
        return filters.Count > 1 ? new LogicalExpression(LogicalOperator.Or, filters) : filters[0];
    }
}
