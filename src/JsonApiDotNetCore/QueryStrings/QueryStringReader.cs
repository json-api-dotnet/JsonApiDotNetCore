using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Diagnostics;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Serialization.Objects;
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
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(queryStringAccessor);
        ArgumentNullException.ThrowIfNull(parameterReaders);

        _options = options;
        _queryStringAccessor = queryStringAccessor;
        _parameterReaders = parameterReaders as IQueryStringParameterReader[] ?? parameterReaders.ToArray();
        _logger = loggerFactory.CreateLogger<QueryStringReader>();
    }

    /// <inheritdoc />
    public void ReadAll(DisableQueryStringAttribute? disableQueryStringAttribute)
    {
        using IDisposable _ = CodeTimingSessionManager.Current.Measure("Parse query string");

        List<InvalidQueryStringParameterException> exceptions = [];

        foreach ((string parameterName, StringValues parameterValue) in _queryStringAccessor.Query)
        {
            if (parameterName.Length > 0)
            {
                try
                {
                    ReadSingle(parameterName, parameterValue, disableQueryStringAttribute ?? DisableQueryStringAttribute.Empty);
                }
                catch (InvalidQueryStringParameterException exception)
                {
                    LogParameterFailedToRead(exception, parameterName);

                    exceptions.Add(exception);
                }
            }
        }

        if (exceptions.Count > 0)
        {
            List<ErrorObject> errors = GetErrors(exceptions);
            throw new JsonApiException(errors, new AggregateException(exceptions));
        }
    }

    private void ReadSingle(string parameterName, StringValues parameterValue, DisableQueryStringAttribute disableQueryStringAttribute)
    {
        IQueryStringParameterReader? reader = _parameterReaders.FirstOrDefault(nextReader => nextReader.CanRead(parameterName));

        if (reader != null)
        {
            LogParameterAccepted(parameterName, parameterValue, reader.GetType().Name);

            if (!reader.AllowEmptyValue && string.IsNullOrEmpty(parameterValue))
            {
                throw new InvalidQueryStringParameterException(parameterName, "Missing query string parameter value.",
                    $"Missing value for '{parameterName}' query string parameter.");
            }

            if (!reader.IsEnabled(disableQueryStringAttribute))
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

    private List<ErrorObject> GetErrors(List<InvalidQueryStringParameterException> exceptions)
    {
        var errors = new List<ErrorObject>(exceptions.SelectMany(exception => exception.Errors).Count());

        foreach (InvalidQueryStringParameterException exception in exceptions)
        {
            foreach (ErrorObject error in exception.Errors)
            {
                if (_options.IncludeExceptionStackTraceInErrors)
                {
                    error.TryIncludeStackTrace(exception);
                }

                errors.Add(error);
            }
        }

        return errors;
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Query string parameter '{ParameterName}' with value '{ParameterValue}' was accepted by {ReaderType}.")]
    private partial void LogParameterAccepted(string parameterName, StringValues parameterValue, string readerType);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Query string parameter '{ParameterName}' was successfully read.")]
    private partial void LogParameterRead(string parameterName);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Failed to read query string parameter '{ParameterName}'.")]
    private partial void LogParameterFailedToRead(Exception exception, string parameterName);
}
