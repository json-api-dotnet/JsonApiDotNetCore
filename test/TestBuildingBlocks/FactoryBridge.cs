using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Xunit.DependencyInjection;

namespace TestBuildingBlocks;

/// <summary>
/// A temporary bridge to prevent adapting all existing tests.
/// </summary>
public sealed class FactoryBridge : IDisposable
{
    private readonly WebApplication _app;
    private readonly ITestOutputHelperAccessor _accessor;
    private readonly bool _captureHttpTraffic;
    private XUnitLogHttpMessageHandler? _handler;

    public IServiceProvider Services => _app.Services;

    internal FactoryBridge(WebApplication app, ITestOutputHelperAccessor accessor, bool captureHttpTraffic)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(accessor);

        _app = app;
        _accessor = accessor;
        _captureHttpTraffic = captureHttpTraffic;
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
            if (_captureHttpTraffic)
            {
                _handler ??= new XUnitLogHttpMessageHandler(_accessor.Output!);
                handlers = [_handler];
            }
        }

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
        for (int i = handlers.Length - 1; i > 0; i--)
        {
            handlers[i - 1].InnerHandler = handlers[i];
        }

        handlers[^1].InnerHandler = serverHandler;
        return new HttpClient(handlers[0]);
    }

    public void Dispose()
    {
        _handler?.Dispose();
    }
}
