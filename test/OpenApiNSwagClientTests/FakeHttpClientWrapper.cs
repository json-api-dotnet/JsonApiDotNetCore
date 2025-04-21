using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using JsonApiDotNetCore.Middleware;

namespace OpenApiNSwagClientTests;

/// <summary>
/// Enables to inject an outgoing response body and inspect the incoming request.
/// </summary>
internal sealed class FakeHttpClientWrapper : IDisposable
{
    private readonly FakeHttpMessageHandler _handler;

    public HttpClient HttpClient { get; }
    public HttpRequestMessage? Request => _handler.Request;
    public string? RequestBody => _handler.RequestBody;

    private FakeHttpClientWrapper(HttpClient httpClient, FakeHttpMessageHandler handler)
    {
        HttpClient = httpClient;
        _handler = handler;
    }

    public JsonElement GetRequestBodyAsJson()
    {
        if (RequestBody == null)
        {
            throw new InvalidOperationException("No body was provided with the request.");
        }

        using JsonDocument jsonDocument = JsonDocument.Parse(RequestBody);
        return jsonDocument.RootElement.Clone();
    }

    public static FakeHttpClientWrapper Create(HttpStatusCode statusCode, string? responseBody)
    {
#pragma warning disable CA2000 // Dispose objects before losing scope
        // Justification: FakeHttpMessageHandler takes ownership, which is owned by FakeHttpClientWrapper.
        HttpResponseMessage response = CreateResponse(statusCode, responseBody);
#pragma warning restore CA2000 // Dispose objects before losing scope
        var handler = new FakeHttpMessageHandler(response);

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost")
        };

        return new FakeHttpClientWrapper(httpClient, handler);
    }

    public void ChangeResponse(HttpStatusCode statusCode, string? responseBody)
    {
        HttpResponseMessage response = CreateResponse(statusCode, responseBody);

        _handler.SetResponse(response);
    }

    private static HttpResponseMessage CreateResponse(HttpStatusCode statusCode, string? responseBody)
    {
        var response = new HttpResponseMessage(statusCode);

        if (!string.IsNullOrEmpty(responseBody))
        {
            response.Content = new StringContent(responseBody, Encoding.UTF8);
            response.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(JsonApiMediaType.Default.ToString());
        }

        return response;
    }

    public void Dispose()
    {
        HttpClient.Dispose();
        _handler.Dispose();
    }

    private sealed class FakeHttpMessageHandler(HttpResponseMessage response) : HttpMessageHandler
    {
        private HttpResponseMessage _response = response;

        public HttpRequestMessage? Request { get; private set; }
        public string? RequestBody { get; private set; }

        public void SetResponse(HttpResponseMessage response)
        {
            ArgumentNullException.ThrowIfNull(response);

            _response = response;
        }

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;

            // Capture the request body here, before it becomes inaccessible because the request has been disposed.
            if (request.Content != null)
            {
                using Stream stream = request.Content.ReadAsStream(cancellationToken);
                using var reader = new StreamReader(stream, Encoding.UTF8);
                RequestBody = reader.ReadToEnd();
            }

            return _response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = Send(request, cancellationToken);
            return Task.FromResult(response);
        }

        protected override void Dispose(bool disposing)
        {
            _response.Dispose();
            base.Dispose(disposing);
        }
    }
}
