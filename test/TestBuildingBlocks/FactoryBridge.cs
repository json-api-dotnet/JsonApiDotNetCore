using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;

namespace TestBuildingBlocks;

/// <summary>
/// A temporary bridge to avoid changing all existing tests.
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
        return _app.GetTestClient();
    }
}
