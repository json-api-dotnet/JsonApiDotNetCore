using System.Net;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Middleware;

/// <inheritdoc cref="IExceptionHandler" />
[PublicAPI]
public partial class ExceptionHandler : IExceptionHandler
{
    private readonly IJsonApiOptions _options;
    private readonly ILogger _logger;

    public ExceptionHandler(ILoggerFactory loggerFactory, IJsonApiOptions options)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentNullException.ThrowIfNull(options);

        _options = options;
        _logger = loggerFactory.CreateLogger<ExceptionHandler>();
    }

    public IReadOnlyList<ErrorObject> HandleException(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        LogException(exception);
        return CreateErrorResponse(exception);
    }

    private void LogException(Exception exception)
    {
        LogLevel level = GetLogLevel(exception);
        string message = GetLogMessage(exception);

        LogException(level, exception, message);
    }

    protected virtual LogLevel GetLogLevel(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        if (exception is OperationCanceledException)
        {
            return LogLevel.None;
        }

        if (exception is JsonApiException and not FailedOperationException)
        {
            return LogLevel.Information;
        }

        return LogLevel.Error;
    }

    protected virtual string GetLogMessage(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception is JsonApiException jsonApiException ? jsonApiException.GetSummary() : exception.Message;
    }

    protected virtual IReadOnlyList<ErrorObject> CreateErrorResponse(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        IReadOnlyList<ErrorObject> errors = exception switch
        {
            JsonApiException jsonApiException => jsonApiException.Errors,
            OperationCanceledException => new[]
            {
                new ErrorObject((HttpStatusCode)499)
                {
                    Title = "Request execution was canceled."
                }
            }.AsReadOnly(),
            _ => new[]
            {
                new ErrorObject(HttpStatusCode.InternalServerError)
                {
                    Title = "An unhandled error occurred while processing this request.",
                    Detail = exception.Message
                }
            }.AsReadOnly()
        };

        if (_options.IncludeExceptionStackTraceInErrors)
        {
            foreach (ErrorObject error in errors)
            {
                error.TryIncludeStackTrace(exception);
            }
        }

        return errors;
    }

    [LoggerMessage(Message = "{Message}")]
    private partial void LogException(LogLevel level, Exception exception, string message);
}
