using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Diagnostics;
using JsonApiDotNetCore.Errors;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.QueryStrings;

/// <inheritdoc cref="IQueryStringReader" />
public sealed partial class QueryStringReader : IQueryStringReader
{
    private readonly IJsonApiOptions _options;
    private readonly IRequestQueryStringAccessor _queryStringAccessor;
    private readonly IQueryStringParameterReader[] _parameterReaders;
    private readonly ILogger<QueryStringReader> _logger;

    public QueryStringReader(IJsonApiOptions options, IRequestQueryStringAccessor queryStringAccessor,
        IEnumerable<IQueryStringParameterReader> parameterReaders, ILoggerFactory loggerFactory)
    {
        ArgumentGuard.NotNull(loggerFactory);
        ArgumentGuard.NotNull(options);
        ArgumentGuard.NotNull(queryStringAccessor);
        ArgumentGuard.NotNull(parameterReaders);

        _options = options;
        _queryStringAccessor = queryStringAccessor;
        _parameterReaders = parameterReaders as IQueryStringParameterReader[] ?? parameterReaders.ToArray();
        _logger = loggerFactory.CreateLogger<QueryStringReader>();
    }

    /// <inheritdoc />
    public void ReadAll(DisableQueryStringAttribute? disableQueryStringAttribute)
    {
        using IDisposable _ = CodeTimingSessionManager.Current.Measure("Parse query string");

        DisableQueryStringAttribute disableQueryStringAttributeNotNull = disableQueryStringAttribute ?? DisableQueryStringAttribute.Empty;

        foreach ((string parameterName, StringValues parameterValue) in _queryStringAccessor.Query)
        {
            if (parameterName.Length == 0)
            {
                continue;
            }

            IQueryStringParameterReader? reader = _parameterReaders.FirstOrDefault(nextReader => nextReader.CanRead(parameterName));

            if (reader != null)
            {
                LogParameterAccepted(parameterName, parameterValue, reader.GetType().Name);

                if (!reader.AllowEmptyValue && string.IsNullOrEmpty(parameterValue))
                {
                    throw new InvalidQueryStringParameterException(parameterName, "Missing query string parameter value.",
                        $"Missing value for '{parameterName}' query string parameter.");
                }

                if (!reader.IsEnabled(disableQueryStringAttributeNotNull))
                {
                    throw new InvalidQueryStringParameterException(parameterName,
                        "Usage of one or more query string parameters is not allowed at the requested endpoint.",
                        $"The parameter '{parameterName}' cannot be used at this endpoint.");
                }

                reader.Read(parameterName, parameterValue);
                LogParameterRead(parameterName);
            }
            else if (!_options.AllowUnknownQueryStringParameters)
            {
                throw new InvalidQueryStringParameterException(parameterName, "Unknown query string parameter.",
                    $"Query string parameter '{parameterName}' is unknown. Set '{nameof(IJsonApiOptions.AllowUnknownQueryStringParameters)}' " +
                    "to 'true' in options to ignore unknown parameters.");
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Query string parameter '{ParameterName}' with value '{ParameterValue}' was accepted by {ReaderType}.")]
    private partial void LogParameterAccepted(string parameterName, StringValues parameterValue, string readerType);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Query string parameter '{ParameterName}' was successfully read.")]
    private partial void LogParameterRead(string parameterName);
}
