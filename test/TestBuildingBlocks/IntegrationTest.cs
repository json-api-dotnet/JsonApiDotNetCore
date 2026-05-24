using System.Net.Http.Headers;
using System.Text.Json;
using FluentAssertions.Extensions;
using Xunit;

namespace TestBuildingBlocks;

/// <summary>
/// A base class for tests that conveniently enables executing HTTP requests against JSON:API endpoints. It throttles tests that are running in parallel
/// to avoid exceeding the maximum active database connections.
/// </summary>
public abstract class IntegrationTest : IAsyncLifetime
{
    private static readonly SemaphoreSlim ThrottleSemaphore = GetDefaultThrottleSemaphore();

    public static DateTimeOffset DefaultDateTimeUtc { get; } = 1.January(2020).At(1, 2, 3).AsUtc();

    protected abstract JsonSerializerOptions SerializerOptions { get; }

    private static SemaphoreSlim GetDefaultThrottleSemaphore()
    {
        int maxConcurrentTestRuns = OperatingSystem.IsWindows() && string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VSAPPIDDIR")) ? 32 : 64;
        return new SemaphoreSlim(maxConcurrentTestRuns);
    }

    public async Task<(HttpResponseMessage httpResponse, TResponseDocument responseDocument)> ExecuteHeadAsync<TResponseDocument>(string requestUrl,
        Action<HttpRequestHeaders>? setRequestHeaders = null)
    {
        using HttpClient httpClient = CreateClient();
        var wrapper = new HttpClientWrapper(httpClient, SerializerOptions);
        return await wrapper.ExecuteHeadAsync<TResponseDocument>(requestUrl, setRequestHeaders);
    }

    public async Task<(HttpResponseMessage httpResponse, TResponseDocument responseDocument)> ExecuteGetAsync<TResponseDocument>(string requestUrl,
        Action<HttpRequestHeaders>? setRequestHeaders = null)
    {
        using HttpClient httpClient = CreateClient();
        var wrapper = new HttpClientWrapper(httpClient, SerializerOptions);
        return await wrapper.ExecuteGetAsync<TResponseDocument>(requestUrl, setRequestHeaders);
    }

    public async Task<(HttpResponseMessage httpResponse, TResponseDocument responseDocument)> ExecutePostAsync<TResponseDocument>(string requestUrl,
        object requestBody, string? contentType = null, Action<HttpRequestHeaders>? setRequestHeaders = null)
    {
        using HttpClient httpClient = CreateClient();
        var wrapper = new HttpClientWrapper(httpClient, SerializerOptions);
        return await wrapper.ExecutePostAsync<TResponseDocument>(requestUrl, requestBody, contentType, setRequestHeaders);
    }

    public async Task<(HttpResponseMessage httpResponse, TResponseDocument responseDocument)> ExecutePostAtomicAsync<TResponseDocument>(string requestUrl,
        object requestBody)
    {
        using HttpClient httpClient = CreateClient();
        var wrapper = new HttpClientWrapper(httpClient, SerializerOptions);
        return await wrapper.ExecutePostAtomicAsync<TResponseDocument>(requestUrl, requestBody);
    }

    public async Task<(HttpResponseMessage httpResponse, TResponseDocument responseDocument)> ExecutePatchAsync<TResponseDocument>(string requestUrl,
        object requestBody, Action<HttpRequestHeaders>? setRequestHeaders = null)
    {
        using HttpClient httpClient = CreateClient();
        var wrapper = new HttpClientWrapper(httpClient, SerializerOptions);
        return await wrapper.ExecutePatchAsync<TResponseDocument>(requestUrl, requestBody, setRequestHeaders);
    }

    public async Task<(HttpResponseMessage httpResponse, TResponseDocument responseDocument)> ExecuteDeleteAsync<TResponseDocument>(string requestUrl,
        object? requestBody = null, Action<HttpRequestHeaders>? setRequestHeaders = null)
    {
        using HttpClient httpClient = CreateClient();
        var wrapper = new HttpClientWrapper(httpClient, SerializerOptions);
        return await wrapper.ExecuteDeleteAsync<TResponseDocument>(requestUrl, requestBody, setRequestHeaders);
    }

    protected abstract HttpClient CreateClient();

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
