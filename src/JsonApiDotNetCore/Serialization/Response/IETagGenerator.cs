using Microsoft.Net.Http.Headers;

namespace JsonApiDotNetCore.Serialization.Response
{
    /// <summary>
    /// Provides generation of an ETag HTTP response header.
    /// </summary>
    public interface IETagGenerator
    {
        /// <summary>
        /// Generates an ETag HTTP response header value for the response to an incoming request.
        /// </summary>
        /// <param name="requestUrl">
        /// The incoming request URL, including query string.
        /// </param>
        /// <param name="responseBody">
        /// The produced response body.
        /// </param>
        /// <returns>
        /// The ETag, or <c>null</c> to disable saving bandwidth.
        /// </returns>
        public EntityTagHeaderValue Generate(string requestUrl, string responseBody);
    }
}
