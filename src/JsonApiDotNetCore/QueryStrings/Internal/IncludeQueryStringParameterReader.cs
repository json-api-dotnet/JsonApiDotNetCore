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
public class IncludeQueryStringParameterReader : QueryStringParameterReader, IIncludeQueryStringParameterReader
{
    private readonly IJsonApiOptions _options;
    private readonly IncludeParser _includeParser;

    private IncludeExpression? _includeExpression;

    public bool AllowEmptyValue => true;

    public IncludeQueryStringParameterReader(IJsonApiRequest request, IResourceGraph resourceGraph, IJsonApiOptions options)
        : base(request, resourceGraph)
    {
        ArgumentGuard.NotNull(options);

        _options = options;
        _includeParser = new IncludeParser();
    }

    /// <inheritdoc />
    public virtual bool IsEnabled(DisableQueryStringAttribute disableQueryStringAttribute)
    {
        ArgumentGuard.NotNull(disableQueryStringAttribute);

        return !IsAtomicOperationsRequest && !disableQueryStringAttribute.ContainsParameter(JsonApiQueryStringParameters.Include);
    }

    /// <inheritdoc />
    public virtual bool CanRead(string parameterName)
    {
        return parameterName == "include";
    }

    /// <inheritdoc />
    public virtual void Read(string parameterName, StringValues parameterValue)
    {
        try
        {
            _includeExpression = GetInclude(parameterValue);
        }
        catch (QueryParseException exception)
        {
            throw new InvalidQueryStringParameterException(parameterName, "The specified include is invalid.", exception.Message, exception);
        }
    }

    private IncludeExpression GetInclude(string parameterValue)
    {
        return _includeParser.Parse(parameterValue, RequestResourceType, _options.MaximumIncludeDepth);
    }

    /// <inheritdoc />
    public virtual IReadOnlyCollection<ExpressionInScope> GetConstraints()
    {
        ExpressionInScope expressionInScope = _includeExpression != null
            ? new ExpressionInScope(null, _includeExpression)
            : new ExpressionInScope(null, IncludeExpression.Empty);

        return expressionInScope.AsArray();
    }
}
