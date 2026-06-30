using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;

namespace TestBuildingBlocks;

/// <summary>
/// A temporary bridge to prevent adapting all existing tests.
/// </summary>
public sealed class FactoryBridge
{
    private readonly WebApplication _app;

    public IServiceProvider Services => _app.Services;

    internal FactoryBridge(WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        _app = app;
    }

    public HttpClient CreateClient()
    {
        return GetTestClient();
    }

    public HttpClient CreateDefaultClient(params DelegatingHandler[] handlers)
    {
        return GetTestClient(handlers);
    }

    public HttpClient GetTestClient(params DelegatingHandler[] handlers)
    {
        if (handlers.Length == 0)
        {
            return _app.GetTestClient();
        }

        TestServer testServer = _app.GetTestServer();
        HttpMessageHandler serverHandler = testServer.CreateHandler();
        HttpClient httpClient = CreateHttpClient(serverHandler, handlers);

        httpClient.BaseAddress ??= new Uri("http://localhost");
        return httpClient;
    }

    private static HttpClient CreateHttpClient(HttpMessageHandler serverHandler, params DelegatingHandler[] handlers)
    {
        for (int index = handlers.Length - 1; index > 0; index--)
        {
            handlers[index - 1].InnerHandler = handlers[index];
        }

        handlers[^1].InnerHandler = serverHandler;
        return new HttpClient(handlers[0]);
    }
}
