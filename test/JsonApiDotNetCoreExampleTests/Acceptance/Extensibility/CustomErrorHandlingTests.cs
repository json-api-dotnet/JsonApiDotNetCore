using System;
using System.Linq;
using System.Net;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.Logging;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Extensibility
{
    public sealed class CustomErrorHandlingTests
    {
        [Fact]
        public void When_using_custom_exception_handler_it_must_create_error_document_and_log()
        {
            // Arrange
            var loggerFactory = new FakeLoggerFactory();
            var options = new JsonApiOptions {IncludeExceptionStackTraceInErrors = true};
            var handler = new CustomExceptionHandler(loggerFactory, options);

            // Act
            var errorDocument = handler.HandleException(new NoPermissionException("YouTube"));

            // Assert
            Assert.Single(errorDocument.Errors);
            Assert.Equal("For support, email to: support@company.com?subject=YouTube",
                errorDocument.Errors[0].Meta.Data["support"]);
            Assert.NotEmpty((string[]) errorDocument.Errors[0].Meta.Data["StackTrace"]);

            Assert.Single(loggerFactory.Logger.Messages);
            Assert.Equal(LogLevel.Warning, loggerFactory.Logger.Messages.Single().LogLevel);
            Assert.Contains("Access is denied.", loggerFactory.Logger.Messages.Single().Text);
        }

        public class CustomExceptionHandler : ExceptionHandler
        {
            public CustomExceptionHandler(ILoggerFactory loggerFactory, IJsonApiOptions options)
                : base(loggerFactory, options)
            {
            }

            protected override LogLevel GetLogLevel(Exception exception)
            {
                if (exception is NoPermissionException)
                {
                    return LogLevel.Warning;
                }

                return base.GetLogLevel(exception);
            }

            protected override ErrorDocument CreateErrorDocument(Exception exception)
            {
                if (exception is NoPermissionException noPermissionException)
                {
                    noPermissionException.Errors[0].Meta.Data.Add("support",
                        "For support, email to: support@company.com?subject=" + noPermissionException.CustomerCode);
                }

                return base.CreateErrorDocument(exception);
            }
        }

        public class NoPermissionException : JsonApiException
        {
            public string CustomerCode { get; }

            public NoPermissionException(string customerCode) : base(new Error(HttpStatusCode.Forbidden)
            {
                Title = "Access is denied.",
                Detail = $"Customer '{customerCode}' does not have permission to access this location."
            })
            {
                CustomerCode = customerCode;
            }
        }
    }
}
