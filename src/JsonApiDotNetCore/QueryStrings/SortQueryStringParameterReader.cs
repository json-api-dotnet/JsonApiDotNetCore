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

/// <inheritdoc cref="ISortQueryStringParameterReader" />
[PublicAPI]
public class SortQueryStringParameterReader : QueryStringParameterReader, ISortQueryStringParameterReader
{
    private readonly IQueryStringParameterScopeParser _scopeParser;
    private readonly ISortParser _sortParser;
    private readonly List<ExpressionInScope> _constraints = [];

    public bool AllowEmptyValue => false;

    public SortQueryStringParameterReader(IQueryStringParameterScopeParser scopeParser, ISortParser sortParser, IJsonApiRequest request,
        IResourceGraph resourceGraph)
        : base(request, resourceGraph)
    {
        ArgumentNullException.ThrowIfNull(scopeParser);
        ArgumentNullException.ThrowIfNull(sortParser);

        _scopeParser = scopeParser;
        _sortParser = sortParser;
    }

    /// <inheritdoc />
    public virtual bool IsEnabled(DisableQueryStringAttribute disableQueryStringAttribute)
    {
        ArgumentNullException.ThrowIfNull(disableQueryStringAttribute);

        return !IsAtomicOperationsRequest && !disableQueryStringAttribute.ContainsParameter(JsonApiQueryStringParameters.Sort);
    }

    /// <inheritdoc />
    public virtual bool CanRead(string parameterName)
    {
        ArgumentException.ThrowIfNullOrEmpty(parameterName);

        bool isNested = parameterName.StartsWith("sort[", StringComparison.Ordinal) && parameterName.EndsWith(']');
        return parameterName == "sort" || isNested;
    }

    /// <inheritdoc />
    public virtual void Read(string parameterName, StringValues parameterValue)
    {
        bool parameterNameIsValid = false;

        try
        {
            IncludeExpression? scopeInclude = GetScope(parameterName);
            parameterNameIsValid = true;

            foreach (ResourceFieldChainExpression? scopeChain in scopeInclude == null
                ? FieldChainInGlobalScope
                : IncludeChainConverter.Instance.GetRelationshipChains(scopeInclude))
            {
                SortExpression sort = GetSort(parameterValue.ToString(), scopeChain);
                var expressionInScope = new ExpressionInScope(scopeChain, sort);
                _constraints.Add(expressionInScope);
            }
        }
        catch (QueryParseException exception)
        {
            string specificMessage = exception.GetMessageWithPosition(parameterNameIsValid ? parameterValue.ToString() : parameterName);
            throw new InvalidQueryStringParameterException(parameterName, "The specified sort is invalid.", specificMessage, exception);
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

    private SortExpression GetSort(string parameterValue, ResourceFieldChainExpression? scope)
    {
        ResourceType resourceTypeInScope = GetResourceTypeForScope(scope);
        return _sortParser.Parse(parameterValue, resourceTypeInScope);
    }

    /// <inheritdoc />
    public virtual IReadOnlyCollection<ExpressionInScope> GetConstraints()
    {
        return _constraints.AsReadOnly();
    }
}
