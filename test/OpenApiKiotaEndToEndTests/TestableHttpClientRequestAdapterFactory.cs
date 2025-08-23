using JsonApiDotNetCore.OpenApi.Client.Kiota;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware;
using TestBuildingBlocks;
using Xunit.Abstractions;

namespace OpenApiKiotaEndToEndTests;

internal sealed class TestableHttpClientRequestAdapterFactory : IDisposable
{
    private readonly HeadersInspectionHandler _headersInspectionHandler = new();
    private readonly SetQueryStringHttpMessageHandler _queryStringMessageHandler = new();
    private readonly XUnitLogHttpMessageHandler _logHttpMessageHandler;

    public TestableHttpClientRequestAdapterFactory(ITestOutputHelper testOutputHelper)
    {
        ArgumentNullException.ThrowIfNull(testOutputHelper);

        _logHttpMessageHandler = new XUnitLogHttpMessageHandler(testOutputHelper);
    }

    public HttpClientRequestAdapter CreateAdapter<TStartup>(WebApplicationFactory<TStartup> webApplicationFactory)
        where TStartup : class
    {
        ArgumentNullException.ThrowIfNull(webApplicationFactory);

        DelegatingHandler[] handlers =
        [
            _headersInspectionHandler,
            _queryStringMessageHandler,
            _logHttpMessageHandler
        ];

        HttpClient httpClient = webApplicationFactory.CreateDefaultClient(handlers);
        return new HttpClientRequestAdapter(new AnonymousAuthenticationProvider(), httpClient: httpClient);
    }

    public IDisposable WithQueryString(IDictionary<string, string?> queryString)
    {
        return _queryStringMessageHandler.CreateScope(queryString);
    }

    public void Dispose()
    {
        _logHttpMessageHandler.Dispose();
        _queryStringMessageHandler.Dispose();
    }
}
