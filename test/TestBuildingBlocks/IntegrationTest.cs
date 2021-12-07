using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Middleware;

namespace TestBuildingBlocks
{
    /// <summary>
    /// A base class for tests that conveniently enables to execute HTTP requests against JSON:API endpoints.
    /// </summary>
    public abstract class IntegrationTest
    {
        // Throttles the number of HTTP requests that run concurrently, in an attempt to
        // prevent PostgreSQL from failing due to too many open connections at the same time.
        // https://github.com/xunit/xunit/issues/2003
        private static readonly SemaphoreSlim ThrottleRequestsSemaphore = new(10);

        protected abstract JsonSerializerOptions SerializerOptions { get; }

        public async Task<(HttpResponseMessage httpResponse, TResponseDocument responseDocument)> ExecuteHeadAsync<TResponseDocument>(string requestUrl,
            Action<HttpRequestHeaders>? setRequestHeaders = null)
        {
            return await ExecuteRequestAsync<TResponseDocument>(HttpMethod.Head, requestUrl, null, null, setRequestHeaders);
        }

        public async Task<(HttpResponseMessage httpResponse, TResponseDocument responseDocument)> ExecuteGetAsync<TResponseDocument>(string requestUrl,
            Action<HttpRequestHeaders>? setRequestHeaders = null)
        {
            return await ExecuteRequestAsync<TResponseDocument>(HttpMethod.Get, requestUrl, null, null, setRequestHeaders);
        }

        public async Task<(HttpResponseMessage httpResponse, TResponseDocument responseDocument)> ExecutePostAsync<TResponseDocument>(string requestUrl,
            object requestBody, string contentType = HeaderConstants.MediaType, Action<HttpRequestHeaders>? setRequestHeaders = null)
        {
            return await ExecuteRequestAsync<TResponseDocument>(HttpMethod.Post, requestUrl, requestBody, contentType, setRequestHeaders);
        }

        public async Task<(HttpResponseMessage httpResponse, TResponseDocument responseDocument)> ExecutePostAtomicAsync<TResponseDocument>(string requestUrl,
            object requestBody, string contentType = HeaderConstants.AtomicOperationsMediaType, Action<HttpRequestHeaders>? setRequestHeaders = null)
        {
            return await ExecuteRequestAsync<TResponseDocument>(HttpMethod.Post, requestUrl, requestBody, contentType, setRequestHeaders);
        }

        public async Task<(HttpResponseMessage httpResponse, TResponseDocument responseDocument)> ExecutePatchAsync<TResponseDocument>(string requestUrl,
            object requestBody, string contentType = HeaderConstants.MediaType, Action<HttpRequestHeaders>? setRequestHeaders = null)
        {
            return await ExecuteRequestAsync<TResponseDocument>(HttpMethod.Patch, requestUrl, requestBody, contentType, setRequestHeaders);
        }

        public async Task<(HttpResponseMessage httpResponse, TResponseDocument responseDocument)> ExecuteDeleteAsync<TResponseDocument>(string requestUrl,
            object? requestBody = null, string contentType = HeaderConstants.MediaType, Action<HttpRequestHeaders>? setRequestHeaders = null)
        {
            return await ExecuteRequestAsync<TResponseDocument>(HttpMethod.Delete, requestUrl, requestBody, contentType, setRequestHeaders);
        }

        private async Task<(HttpResponseMessage httpResponse, TResponseDocument responseDocument)> ExecuteRequestAsync<TResponseDocument>(HttpMethod method,
            string requestUrl, object? requestBody, string? contentType, Action<HttpRequestHeaders>? setRequestHeaders)
        {
            using var request = new HttpRequestMessage(method, requestUrl);
            string? requestText = SerializeRequest(requestBody);

            if (!string.IsNullOrEmpty(requestText))
            {
                requestText = requestText.Replace("atomic__", "atomic:");
                request.Content = new StringContent(requestText);
                request.Content.Headers.ContentLength = Encoding.UTF8.GetByteCount(requestText);

                if (contentType != null)
                {
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
                }
            }

            setRequestHeaders?.Invoke(request.Headers);

            using HttpClient client = CreateClient();
            HttpResponseMessage responseMessage = await SendAsync(client, request);

            string responseText = await responseMessage.Content.ReadAsStringAsync();
            var responseDocument = DeserializeResponse<TResponseDocument>(responseText);

            return (responseMessage, responseDocument!);
        }

        private static async Task<HttpResponseMessage> SendAsync(HttpClient client, HttpRequestMessage request)
        {
            await ThrottleRequestsSemaphore.WaitAsync();

            try
            {
                return await client.SendAsync(request);
            }
            finally
            {
                ThrottleRequestsSemaphore.Release();
            }
        }

        private string? SerializeRequest(object? requestBody)
        {
            return requestBody == null ? null :
                requestBody is string stringRequestBody ? stringRequestBody : JsonSerializer.Serialize(requestBody, SerializerOptions);
        }

        protected abstract HttpClient CreateClient();

        private TResponseDocument? DeserializeResponse<TResponseDocument>(string responseText)
        {
            if (typeof(TResponseDocument) == typeof(string))
            {
                return (TResponseDocument)(object)responseText;
            }

            if (string.IsNullOrEmpty(responseText))
            {
                return default;
            }

            try
            {
                return JsonSerializer.Deserialize<TResponseDocument>(responseText, SerializerOptions);
            }
            catch (JsonException exception)
            {
                throw new FormatException($"Failed to deserialize response body to JSON:\n{responseText}", exception);
            }
        }
    }
}
