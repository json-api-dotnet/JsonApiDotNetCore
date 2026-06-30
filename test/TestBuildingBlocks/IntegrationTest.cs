using System.Net.Http.Headers;
using System.Text.Json;
using FluentAssertions.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace TestBuildingBlocks;

/// <summary>
/// A base class for tests that conveniently enables executing HTTP requests against JSON:API endpoints.
/// </summary>
/// <remarks>
/// Tests that use a database should call <see cref="AcquireDatabaseThrottleAsync" /> and <see cref="ReleaseDatabaseThrottle" /> to avoid exceeding the
/// maximum active database connections.
/// </remarks>
public abstract class IntegrationTest
{
    private static readonly SemaphoreSlim DatabaseThrottleSemaphore = CreateDatabaseThrottleSemaphore();
    protected static readonly Action<ServiceProviderOptions> ConfigureServiceProvider = static options => options.ValidateScopes = true;

    public static DateTimeOffset DefaultDateTimeUtc { get; } = 1.January(2020).At(1, 2, 3).AsUtc();

    protected abstract JsonSerializerOptions SerializerOptions { get; }

    private static SemaphoreSlim CreateDatabaseThrottleSemaphore()
    {
        int maxConcurrentTestRuns = OperatingSystem.IsWindows() && string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VSAPPIDDIR")) ? 32 : 64;
        return new SemaphoreSlim(maxConcurrentTestRuns);
    }

    protected async Task AcquireDatabaseThrottleAsync()
    {
        await DatabaseThrottleSemaphore.WaitAsync();
    }

    protected void ReleaseDatabaseThrottle()
    {
        DatabaseThrottleSemaphore.Release();
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
}
