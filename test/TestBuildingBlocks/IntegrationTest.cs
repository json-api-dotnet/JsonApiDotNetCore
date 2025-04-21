using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FluentAssertions.Extensions;
using JsonApiDotNetCore.Middleware;
using Xunit;

namespace TestBuildingBlocks;

/// <summary>
/// A base class for tests that conveniently enables to execute HTTP requests against JSON:API endpoints. It throttles tests that are running in parallel
/// to avoid exceeding the maximum active database connections.
/// </summary>
public abstract class IntegrationTest : IAsyncLifetime
{
    private static readonly MediaTypeHeaderValue DefaultMediaType = MediaTypeHeaderValue.Parse(JsonApiMediaType.Default.ToString());

    private static readonly MediaTypeWithQualityHeaderValue OperationsMediaType =
        MediaTypeWithQualityHeaderValue.Parse(JsonApiMediaType.AtomicOperations.ToString());

    private static readonly SemaphoreSlim ThrottleSemaphore = GetDefaultThrottleSemaphore();

    public static DateTimeOffset DefaultDateTimeUtc { get; } = 1.January(2020).At(1, 2, 3).AsUtc();

    protected abstract JsonSerializerOptions SerializerOptions { get; }

    private static SemaphoreSlim GetDefaultThrottleSemaphore()
    {
        int maxConcurrentTestRuns = OperatingSystem.IsWindows() && Environment.GetEnvironmentVariable("CI") != null ? 32 : 64;
        return new SemaphoreSlim(maxConcurrentTestRuns);
    }

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

#pragma warning disable AV1553 // Do not use optional parameters with default value null for strings, collections or tasks
    public async Task<(HttpResponseMessage httpResponse, TResponseDocument responseDocument)> ExecutePostAsync<TResponseDocument>(string requestUrl,
        object requestBody, string? contentType = null, Action<HttpRequestHeaders>? setRequestHeaders = null)
#pragma warning restore AV1553 // Do not use optional parameters with default value null for strings, collections or tasks
    {
        MediaTypeHeaderValue mediaType = contentType == null ? DefaultMediaType : MediaTypeHeaderValue.Parse(contentType);

        return await ExecuteRequestAsync<TResponseDocument>(HttpMethod.Post, requestUrl, requestBody, mediaType, setRequestHeaders);
    }

    public async Task<(HttpResponseMessage httpResponse, TResponseDocument responseDocument)> ExecutePostAtomicAsync<TResponseDocument>(string requestUrl,
        object requestBody)
    {
        Action<HttpRequestHeaders> setRequestHeaders = headers => headers.Accept.Add(OperationsMediaType);

        return await ExecuteRequestAsync<TResponseDocument>(HttpMethod.Post, requestUrl, requestBody, OperationsMediaType, setRequestHeaders);
    }

    public async Task<(HttpResponseMessage httpResponse, TResponseDocument responseDocument)> ExecutePatchAsync<TResponseDocument>(string requestUrl,
        object requestBody, Action<HttpRequestHeaders>? setRequestHeaders = null)
    {
        return await ExecuteRequestAsync<TResponseDocument>(HttpMethod.Patch, requestUrl, requestBody, DefaultMediaType, setRequestHeaders);
    }

    public async Task<(HttpResponseMessage httpResponse, TResponseDocument responseDocument)> ExecuteDeleteAsync<TResponseDocument>(string requestUrl,
        object? requestBody = null, Action<HttpRequestHeaders>? setRequestHeaders = null)
    {
        return await ExecuteRequestAsync<TResponseDocument>(HttpMethod.Delete, requestUrl, requestBody, DefaultMediaType, setRequestHeaders);
    }

    private async Task<(HttpResponseMessage httpResponse, TResponseDocument responseDocument)> ExecuteRequestAsync<TResponseDocument>(HttpMethod method,
        string requestUrl, object? requestBody, MediaTypeHeaderValue? contentType, Action<HttpRequestHeaders>? setRequestHeaders)
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
                request.Content.Headers.ContentType = contentType;
            }
        }

        setRequestHeaders?.Invoke(request.Headers);

        using HttpClient client = CreateClient();
        HttpResponseMessage responseMessage = await client.SendAsync(request);

        string responseText = await responseMessage.Content.ReadAsStringAsync();
        var responseDocument = DeserializeResponse<TResponseDocument>(responseText);

        return (responseMessage, responseDocument!);
    }

    private string? SerializeRequest(object? requestBody)
    {
        return requestBody == null ? null : requestBody as string ?? JsonSerializer.Serialize(requestBody, SerializerOptions);
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

    public async Task InitializeAsync()
    {
        await ThrottleSemaphore.WaitAsync();
    }

    public virtual Task DisposeAsync()
    {
        _ = ThrottleSemaphore.Release();
        return Task.CompletedTask;
    }
}
