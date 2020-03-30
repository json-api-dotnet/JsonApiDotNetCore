using System.Net;

namespace JsonApiDotNetCore.Internal
{
    internal static class Exceptions
    {
        private const string DOCUMENTATION_URL = "https://json-api-dotnet.github.io/#/errors/";
        private static string BuildUrl(string title) => DOCUMENTATION_URL + title;

        public static JsonApiException UnSupportedRequestMethod { get; }  
            = new JsonApiException(HttpStatusCode.MethodNotAllowed, "Request method is not supported.", BuildUrl(nameof(UnSupportedRequestMethod)));
    }
}
