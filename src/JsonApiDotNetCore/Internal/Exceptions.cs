using System.Net;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Internal
{
    internal static class Exceptions
    {
        private const string DOCUMENTATION_URL = "https://json-api-dotnet.github.io/#/errors/";
        private static string BuildUrl(string title) => DOCUMENTATION_URL + title;

        public static JsonApiException UnSupportedRequestMethod { get; }
            = new JsonApiException(new Error(HttpStatusCode.Conflict)
            {
               Title = "Request method is not supported.",
               Detail = BuildUrl(nameof(UnSupportedRequestMethod))
            });
    }
}
