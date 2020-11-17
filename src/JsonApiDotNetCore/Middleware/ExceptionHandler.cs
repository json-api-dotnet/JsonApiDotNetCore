using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Middleware
{
    /// <inheritdoc />
    public class ExceptionHandler : IExceptionHandler
    {
        private readonly IJsonApiOptions _options;
        private readonly ILogger _logger;

        public ExceptionHandler(ILoggerFactory loggerFactory, IJsonApiOptions options)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = loggerFactory.CreateLogger<ExceptionHandler>();
        }

        public ErrorDocument HandleException(Exception exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            Exception demystified = exception.Demystify();

            LogException(demystified);

            return CreateErrorDocument(demystified);
        }

        private void LogException(Exception exception)
        {
            var level = GetLogLevel(exception);
            var message = GetLogMessage(exception);
            
            _logger.Log(level, exception, message);
        }

        protected virtual LogLevel GetLogLevel(Exception exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            if (exception is OperationCanceledException)
            {
                return LogLevel.None;
            }

            if (exception is JsonApiException || exception is InvalidModelStateException)
            {
                return LogLevel.Information;
            }

            return LogLevel.Error;
        }

        protected virtual string GetLogMessage(Exception exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            return exception is JsonApiException jsonApiException
                ? jsonApiException.Error.Title
                : exception.Message;
        }

        protected virtual ErrorDocument CreateErrorDocument(Exception exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            if (exception is IHasMultipleErrors exceptionWithMultipleErrors)
            {
                return new ErrorDocument(exceptionWithMultipleErrors.Errors);
            }

            Error error = exception is JsonApiException jsonApiException
                ? jsonApiException.Error
                : exception is TaskCanceledException
                    ? new Error((HttpStatusCode) 499)
                    {
                        Title = "Request execution was canceled."
                    }
                    : new Error(HttpStatusCode.InternalServerError)
                    {
                        Title = "An unhandled error occurred while processing this request.",
                        Detail = exception.Message
                    };

            ApplyOptions(error, exception);

            return new ErrorDocument(error);
        }

        private void ApplyOptions(Error error, Exception exception)
        {
            error.Meta.IncludeExceptionStackTrace(_options.IncludeExceptionStackTraceInErrors ? exception : null);
        }
    }
}
