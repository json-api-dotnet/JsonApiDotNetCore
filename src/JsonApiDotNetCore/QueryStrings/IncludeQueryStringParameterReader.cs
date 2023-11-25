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

/// <inheritdoc cref="IIncludeQueryStringParameterReader" />
[PublicAPI]
public class IncludeQueryStringParameterReader : QueryStringParameterReader, IIncludeQueryStringParameterReader
{
    private readonly IIncludeParser _includeParser;

    private IncludeExpression? _includeExpression;

    public bool AllowEmptyValue => true;

    public IncludeQueryStringParameterReader(IIncludeParser includeParser, IJsonApiRequest request, IResourceGraph resourceGraph)
        : base(request, resourceGraph)
    {
        ArgumentGuard.NotNull(includeParser);

        _includeParser = includeParser;
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
            // Workaround for https://youtrack.jetbrains.com/issue/RSRP-493256/Incorrect-possible-null-assignment
            // ReSharper disable once AssignNullToNotNullAttribute
            _includeExpression = GetInclude(parameterValue.ToString());
        }
        catch (QueryParseException exception)
        {
            // Workaround for https://youtrack.jetbrains.com/issue/RSRP-493256/Incorrect-possible-null-assignment
            // ReSharper disable once AssignNullToNotNullAttribute
            string specificMessage = exception.GetMessageWithPosition(parameterValue.ToString());
            throw new InvalidQueryStringParameterException(parameterName, "The specified include is invalid.", specificMessage, exception);
        }
    }

    private IncludeExpression GetInclude(string parameterValue)
    {
        return _includeParser.Parse(parameterValue, RequestResourceType);
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
