using System;
using System.Net;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Exceptions;
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

            _logger.Log(level, exception, exception.Message);
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
            if (_options.IncludeExceptionStackTraceInErrors)
            {
                error.Meta.IncludeExceptionStackTrace(exception);
            }
        }
    }
}
