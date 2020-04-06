using System;
using System.Diagnostics;
using System.Net;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Exceptions;
using JsonApiDotNetCore.Models.JsonApiDocuments;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Middleware
{
    public class DefaultExceptionHandler : IExceptionHandler
    {
        private readonly IJsonApiOptions _options;
        private readonly ILogger _logger;

        public DefaultExceptionHandler(ILoggerFactory loggerFactory, IJsonApiOptions options)
        {
            _options = options;
            _logger = loggerFactory.CreateLogger<DefaultExceptionHandler>();
        }

        public ErrorDocument HandleException(Exception exception)
        {
            LogException(exception);

            return CreateErrorDocument(exception);
        }

        private void LogException(Exception exception)
        {
            var level = GetLogLevel(exception);

            Exception demystified = exception.Demystify();
            _logger.Log(level, demystified, $"Intercepted {demystified.GetType().Name}: {demystified.Message}");
        }

        protected virtual LogLevel GetLogLevel(Exception exception)
        {
            if (exception is JsonApiException || exception is InvalidModelStateException)
            {
                return LogLevel.Information;
            }

            return LogLevel.Error;
        }

        protected virtual ErrorDocument CreateErrorDocument(Exception exception)
        {
            if (exception is InvalidModelStateException modelStateException)
            {
                return new ErrorDocument(modelStateException.Errors);
            }

            Error error = exception is JsonApiException jsonApiException
                ? jsonApiException.Error
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
