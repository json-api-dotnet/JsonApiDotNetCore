using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JsonApiDotNetCore.Middleware;
using Newtonsoft.Json;

namespace TestBuildingBlocks
{
    /// <summary>
    /// A base class for tests that conveniently enables to execute HTTP requests against json:api endpoints.
    /// </summary>
    public abstract class IntegrationTest
    {
        public async Task<(HttpResponseMessage httpResponse, TResponseDocument responseDocument)>
            ExecuteGetAsync<TResponseDocument>(string requestUrl,
                IEnumerable<MediaTypeWithQualityHeaderValue> acceptHeaders = null)
        {
            return await ExecuteRequestAsync<TResponseDocument>(HttpMethod.Get, requestUrl, null, null, acceptHeaders);
        }

        public async Task<(HttpResponseMessage httpResponse, TResponseDocument responseDocument)>
            ExecutePostAsync<TResponseDocument>(string requestUrl, object requestBody,
                string contentType = HeaderConstants.MediaType,
                IEnumerable<MediaTypeWithQualityHeaderValue> acceptHeaders = null)
        {
            return await ExecuteRequestAsync<TResponseDocument>(HttpMethod.Post, requestUrl, requestBody, contentType,
                acceptHeaders);
        }

        public async Task<(HttpResponseMessage httpResponse, TResponseDocument responseDocument)>
            ExecutePostAtomicAsync<TResponseDocument>(string requestUrl, object requestBody,
                string contentType = HeaderConstants.AtomicOperationsMediaType,
                IEnumerable<MediaTypeWithQualityHeaderValue> acceptHeaders = null)
        {
            return await ExecuteRequestAsync<TResponseDocument>(HttpMethod.Post, requestUrl, requestBody, contentType,
                acceptHeaders);
        }

        public async Task<(HttpResponseMessage httpResponse, TResponseDocument responseDocument)>
            ExecutePatchAsync<TResponseDocument>(string requestUrl, object requestBody,
                string contentType = HeaderConstants.MediaType,
                IEnumerable<MediaTypeWithQualityHeaderValue> acceptHeaders = null)
        {
            return await ExecuteRequestAsync<TResponseDocument>(HttpMethod.Patch, requestUrl, requestBody, contentType,
                acceptHeaders);
        }

        public async Task<(HttpResponseMessage httpResponse, TResponseDocument responseDocument)>
            ExecuteDeleteAsync<TResponseDocument>(string requestUrl, object requestBody = null,
                string contentType = HeaderConstants.MediaType,
                IEnumerable<MediaTypeWithQualityHeaderValue> acceptHeaders = null)
        {
            return await ExecuteRequestAsync<TResponseDocument>(HttpMethod.Delete, requestUrl, requestBody, contentType,
                acceptHeaders);
        }

        private async Task<(HttpResponseMessage httpResponse, TResponseDocument responseDocument)>
            ExecuteRequestAsync<TResponseDocument>(HttpMethod method, string requestUrl, object requestBody,
                string contentType, IEnumerable<MediaTypeWithQualityHeaderValue> acceptHeaders)
        {
            var request = new HttpRequestMessage(method, requestUrl);
            string requestText = SerializeRequest(requestBody);

            if (!string.IsNullOrEmpty(requestText))
            {
                requestText = requestText.Replace("atomic__", "atomic:");
                request.Content = new StringContent(requestText);

                if (contentType != null)
                {
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
                }
            }

            using HttpClient client = CreateClient();

            if (acceptHeaders != null)
            {
                foreach (var acceptHeader in acceptHeaders)
                {
                    client.DefaultRequestHeaders.Accept.Add(acceptHeader);
                }
            }

            HttpResponseMessage responseMessage = await client.SendAsync(request);

            string responseText = await responseMessage.Content.ReadAsStringAsync();
            var responseDocument = DeserializeResponse<TResponseDocument>(responseText);

            return (responseMessage, responseDocument);
        }

        private string SerializeRequest(object requestBody)
        {
            return requestBody == null
                ? null
                : requestBody is string stringRequestBody
                    ? stringRequestBody
                    : JsonConvert.SerializeObject(requestBody);
        }

        protected abstract HttpClient CreateClient();

        private TResponseDocument DeserializeResponse<TResponseDocument>(string responseText)
        {
            if (typeof(TResponseDocument) == typeof(string))
            {
                return (TResponseDocument)(object)responseText;
            }

            try
            {
                return JsonConvert.DeserializeObject<TResponseDocument>(responseText,
                    IntegrationTestConfiguration.DeserializationSettings);
            }
            catch (JsonException exception)
            {
                throw new FormatException($"Failed to deserialize response body to JSON:\n{responseText}", exception);
            }
        }
    }
}
