using System.Net;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ExceptionHandling
{
    internal sealed class ConsumerArticleIsNoLongerAvailableException : JsonApiException
    {
        public string SupportEmailAddress { get; }

        public ConsumerArticleIsNoLongerAvailableException(string articleCode, string supportEmailAddress)
            : base(new Error(HttpStatusCode.Gone)
            {
                Title = "The requested article is no longer available.",
                Detail = $"Article with code '{articleCode}' is no longer available."
            })
        {
            SupportEmailAddress = supportEmailAddress;
        }
    }
}
