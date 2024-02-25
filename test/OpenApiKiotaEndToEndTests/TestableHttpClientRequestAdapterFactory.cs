using JsonApiDotNetCore.OpenApi.Client.Kiota;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using TestBuildingBlocks;
using Xunit.Abstractions;

namespace OpenApiKiotaEndToEndTests;

internal sealed class TestableHttpClientRequestAdapterFactory
{
    private readonly XUnitLogHttpMessageHandler _logHttpMessageHandler;
    private readonly SetQueryStringHttpMessageHandler _queryStringMessageHandler = new();

    public TestableHttpClientRequestAdapterFactory(ITestOutputHelper testOutputHelper)
    {
        ArgumentNullException.ThrowIfNull(testOutputHelper);

        _logHttpMessageHandler = new XUnitLogHttpMessageHandler(testOutputHelper);
    }

    public HttpClientRequestAdapter CreateAdapter<TStartup>(WebApplicationFactory<TStartup> webApplicationFactory)
        where TStartup : class
    {
        ArgumentNullException.ThrowIfNull(webApplicationFactory);

        IList<DelegatingHandler> delegatingHandlers = KiotaClientFactory.CreateDefaultHandlers();
        delegatingHandlers.Add(_queryStringMessageHandler);
        delegatingHandlers.Add(_logHttpMessageHandler);
        HttpClient httpClient = webApplicationFactory.CreateDefaultClient(delegatingHandlers.ToArray());

        return new HttpClientRequestAdapter(new AnonymousAuthenticationProvider(), httpClient: httpClient);
    }

    public IDisposable WithQueryString(IDictionary<string, string?> queryString)
    {
        return _queryStringMessageHandler.CreateScope(queryString);
    }
}
