using System.Net;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ExceptionHandling
{
    public sealed class ConsumerArticleIsNoLongerAvailableException : JsonApiException
    {
        public string ArticleCode { get; }
        public string SupportEmailAddress { get; }

        public ConsumerArticleIsNoLongerAvailableException(string articleCode, string supportEmailAddress)
            : base(new Error(HttpStatusCode.Gone)
            {
                Title = "The requested article is no longer available.",
                Detail = $"Article with code '{articleCode}' is no longer available."
            })
        {
            ArticleCode = articleCode;
            SupportEmailAddress = supportEmailAddress;
        }
    }
}
