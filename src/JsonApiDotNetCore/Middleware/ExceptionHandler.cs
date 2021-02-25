using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Middleware
{
    /// <inheritdoc />
    [PublicAPI]
    public class ExceptionHandler : IExceptionHandler
    {
        private readonly IJsonApiOptions _options;
        private readonly ILogger _logger;

        public ExceptionHandler(ILoggerFactory loggerFactory, IJsonApiOptions options)
        {
            ArgumentGuard.NotNull(loggerFactory, nameof(loggerFactory));
            ArgumentGuard.NotNull(options, nameof(options));

            _options = options;
            _logger = loggerFactory.CreateLogger<ExceptionHandler>();
        }

        public ErrorDocument HandleException(Exception exception)
        {
            ArgumentGuard.NotNull(exception, nameof(exception));

            Exception demystified = exception.Demystify();

            LogException(demystified);

            return CreateErrorDocument(demystified);
        }

        private void LogException(Exception exception)
        {
            LogLevel level = GetLogLevel(exception);
            string message = GetLogMessage(exception);

            _logger.Log(level, exception, message);
        }

        protected virtual LogLevel GetLogLevel(Exception exception)
        {
            ArgumentGuard.NotNull(exception, nameof(exception));

            if (exception is OperationCanceledException)
            {
                return LogLevel.None;
            }

            if (exception is JsonApiException)
            {
                return LogLevel.Information;
            }

            return LogLevel.Error;
        }

        protected virtual string GetLogMessage(Exception exception)
        {
            ArgumentGuard.NotNull(exception, nameof(exception));

            return exception.Message;
        }

        protected virtual ErrorDocument CreateErrorDocument(Exception exception)
        {
            ArgumentGuard.NotNull(exception, nameof(exception));

            IReadOnlyList<Error> errors = exception is JsonApiException jsonApiException ? jsonApiException.Errors :
                exception is OperationCanceledException ? new Error((HttpStatusCode)499)
                {
                    Title = "Request execution was canceled."
                }.AsArray() : new Error(HttpStatusCode.InternalServerError)
                {
                    Title = "An unhandled error occurred while processing this request.",
                    Detail = exception.Message
                }.AsArray();

            foreach (Error error in errors)
            {
                ApplyOptions(error, exception);
            }

            return new ErrorDocument(errors);
        }

        private void ApplyOptions(Error error, Exception exception)
        {
            Exception resultException = exception is InvalidModelStateException ? null : exception;

            error.Meta.IncludeExceptionStackTrace(_options.IncludeExceptionStackTraceInErrors ? resultException : null);
        }
    }
}
