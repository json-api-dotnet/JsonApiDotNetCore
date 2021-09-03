using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ExceptionHandling
{
    public sealed class AlternateExceptionHandler : ExceptionHandler
    {
        public AlternateExceptionHandler(ILoggerFactory loggerFactory, IJsonApiOptions options)
            : base(loggerFactory, options)
        {
        }

        protected override LogLevel GetLogLevel(Exception exception)
        {
            if (exception is ConsumerArticleIsNoLongerAvailableException)
            {
                return LogLevel.Warning;
            }

            return base.GetLogLevel(exception);
        }

        protected override ErrorDocument CreateErrorDocument(Exception exception)
        {
            if (exception is ConsumerArticleIsNoLongerAvailableException articleException)
            {
                articleException.Errors[0].Meta.Data.Add("support",
                    $"Please contact us for info about similar articles at {articleException.SupportEmailAddress}.");
            }

            return base.CreateErrorDocument(exception);
        }
    }
}
