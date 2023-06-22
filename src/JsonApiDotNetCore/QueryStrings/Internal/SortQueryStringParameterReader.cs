using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Internal.Parsing;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.QueryStrings.Internal;

[PublicAPI]
public class SortQueryStringParameterReader : QueryStringParameterReader, ISortQueryStringParameterReader
{
    private readonly QueryStringParameterScopeParser _scopeParser;
    private readonly SortParser _sortParser;
    private readonly List<ExpressionInScope> _constraints = new();

    public bool AllowEmptyValue => false;

    public SortQueryStringParameterReader(IJsonApiRequest request, IResourceGraph resourceGraph)
        : base(request, resourceGraph)
    {
        _scopeParser = new QueryStringParameterScopeParser(FieldChainRequirements.EndsInToMany);
        _sortParser = new SortParser();
    }

    /// <inheritdoc />
    public virtual bool IsEnabled(DisableQueryStringAttribute disableQueryStringAttribute)
    {
        ArgumentGuard.NotNull(disableQueryStringAttribute);

        return !IsAtomicOperationsRequest && !disableQueryStringAttribute.ContainsParameter(JsonApiQueryStringParameters.Sort);
    }

    /// <inheritdoc />
    public virtual bool CanRead(string parameterName)
    {
        ArgumentGuard.NotNullNorEmpty(parameterName);

        bool isNested = parameterName.StartsWith("sort[", StringComparison.Ordinal) && parameterName.EndsWith("]", StringComparison.Ordinal);
        return parameterName == "sort" || isNested;
    }

    /// <inheritdoc />
    public virtual void Read(string parameterName, StringValues parameterValue)
    {
        bool parameterNameIsValid = false;

        try
        {
            ResourceFieldChainExpression? scope = GetScope(parameterName);
            parameterNameIsValid = true;

            SortExpression sort = GetSort(parameterValue.ToString(), scope);
            var expressionInScope = new ExpressionInScope(scope, sort);
            _constraints.Add(expressionInScope);
        }
        catch (QueryParseException exception)
        {
            string specificMessage = exception.GetMessageWithPosition(parameterNameIsValid ? parameterValue : parameterName);
            throw new InvalidQueryStringParameterException(parameterName, "The specified sort is invalid.", specificMessage, exception);
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

    private SortExpression GetSort(string parameterValue, ResourceFieldChainExpression? scope)
    {
        ResourceType resourceTypeInScope = GetResourceTypeForScope(scope);
        return _sortParser.Parse(parameterValue, resourceTypeInScope);
    }

    /// <inheritdoc />
    public virtual IReadOnlyCollection<ExpressionInScope> GetConstraints()
    {
        return _constraints;
    }
}
