using System.Collections.Immutable;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Parsing;
using JsonApiDotNetCore.QueryStrings.FieldChains;
using JsonApiDotNetCore.Resources.Annotations;
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

        return parameterName == "filter" || IsFilterParameterScoped(parameterName);
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
                ValidateLegacyRelationshipFilter(name, value);
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

    private void ValidateLegacyRelationshipFilter(string parameterName, string parameterValue)
    {
        if (IsFilterParameterScoped(parameterName))
        {
            string fieldChain = parameterName["filter[".Length..^1];

            if (fieldChain.Length > 0 && parameterValue != LegacyFilterNotationConverter.ParameterValuePrefix.IsNull &&
                parameterValue != LegacyFilterNotationConverter.ParameterValuePrefix.IsNotNull)
            {
                PatternMatchResult result = BuiltInPatterns.ToOneOrCompoundChainEndingInToOneOrAttribute.Match(fieldChain, RequestResourceType);

                if (result is { IsSuccess: true, FieldChain.Count: > 0 } && result.FieldChain[^1] is RelationshipAttribute relationship)
                {
                    throw new QueryParseException(
                        $"Filtering on relationship '{relationship.PublicName}' is only supported using '{LegacyFilterNotationConverter.ParameterValuePrefix.IsNull}' " +
                        $"or '{LegacyFilterNotationConverter.ParameterValuePrefix.IsNotNull}'. To filter by related attributes, " +
                        $"use 'filter[{relationship.PublicName}.id]=123' or use expression syntax.", -1);
                }
            }
        }
    }

    private static bool IsFilterParameterScoped(string parameterName)
    {
        return parameterName.StartsWith("filter[", StringComparison.Ordinal) && parameterName.EndsWith(']');
    }

    private IncludeExpression? GetScope(string parameterName)
    {
        QueryStringParameterScopeExpression parameterScope = _scopeParser.Parse(parameterName, RequestResourceType);

        if (parameterScope.Scope == null)
        {
            AssertIsCollectionRequest(parameterName);
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
            yield return new RelationshipChainExpressionInScope(null, filter);
        }

        foreach ((ResourceFieldChainExpression scope, ImmutableArray<FilterExpression>.Builder filtersBuilder) in _filtersPerScope)
        {
            FilterExpression filter = MergeFilters(filtersBuilder.ToImmutable());
            yield return new RelationshipChainExpressionInScope(scope, filter);
        }
    }

    private static FilterExpression MergeFilters(IImmutableList<FilterExpression> filters)
    {
        return filters.Count > 1 ? new LogicalExpression(LogicalOperator.Or, filters) : filters[0];
    }
}
