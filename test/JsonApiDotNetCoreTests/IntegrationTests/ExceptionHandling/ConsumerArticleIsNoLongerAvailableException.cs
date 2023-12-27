using System.Net;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCoreTests.IntegrationTests.ExceptionHandling;

internal sealed class ConsumerArticleIsNoLongerAvailableException(string articleCode, string supportEmailAddress) : JsonApiException(
    new ErrorObject(HttpStatusCode.Gone)
    {
        Title = "The requested article is no longer available.",
        Detail = $"Article with code '{articleCode}' is no longer available."
    })
{
    public string SupportEmailAddress { get; } = supportEmailAddress;
}
