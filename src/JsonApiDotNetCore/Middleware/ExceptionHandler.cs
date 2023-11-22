using System.Diagnostics;
using System.Net;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Middleware;

/// <inheritdoc cref="IExceptionHandler" />
[PublicAPI]
public class ExceptionHandler : IExceptionHandler
{
    private readonly IJsonApiOptions _options;
    private readonly ILogger _logger;

    public ExceptionHandler(ILoggerFactory loggerFactory, IJsonApiOptions options)
    {
        ArgumentGuard.NotNull(loggerFactory);
        ArgumentGuard.NotNull(options);

        _options = options;
        _logger = loggerFactory.CreateLogger<ExceptionHandler>();
    }

    public IReadOnlyList<ErrorObject> HandleException(Exception exception)
    {
        ArgumentGuard.NotNull(exception);

        Exception demystified = exception.Demystify();

        LogException(demystified);

        return CreateErrorResponse(demystified);
    }

    private void LogException(Exception exception)
    {
        LogLevel level = GetLogLevel(exception);
        string message = GetLogMessage(exception);

        _logger.Log(level, exception, message);
    }

    protected virtual LogLevel GetLogLevel(Exception exception)
    {
        ArgumentGuard.NotNull(exception);

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
        ArgumentGuard.NotNull(exception);

        return exception is JsonApiException jsonApiException ? jsonApiException.GetSummary() : exception.Message;
    }

    protected virtual IReadOnlyList<ErrorObject> CreateErrorResponse(Exception exception)
    {
        ArgumentGuard.NotNull(exception);

        IReadOnlyList<ErrorObject> errors = exception switch
        {
            JsonApiException jsonApiException => jsonApiException.Errors,
            OperationCanceledException =>
            [
                new ErrorObject((HttpStatusCode)499)
                {
                    Title = "Request execution was canceled."
                }
            ],
            _ =>
            [
                new ErrorObject(HttpStatusCode.InternalServerError)
                {
                    Title = "An unhandled error occurred while processing this request.",
                    Detail = exception.Message
                }
            ]
        };

        if (_options.IncludeExceptionStackTraceInErrors && exception is not InvalidModelStateException)
        {
            IncludeStackTraces(exception, errors);
        }

        return errors;
    }

    private void IncludeStackTraces(Exception exception, IReadOnlyList<ErrorObject> errors)
    {
        string[] stackTraceLines = exception.ToString().Split(Environment.NewLine);

        if (stackTraceLines.Any())
        {
            foreach (ErrorObject error in errors)
            {
                error.Meta ??= new Dictionary<string, object?>();
                error.Meta["StackTrace"] = stackTraceLines;
            }
        }
    }
}
