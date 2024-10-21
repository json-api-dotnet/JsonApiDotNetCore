using JsonApiDotNetCore;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Serialization.Request.Adapters;

namespace JsonApiDotNetCoreTests.IntegrationTests.ContentNegotiation.CustomExtensions;

internal sealed class CapturingDocumentAdapter : IDocumentAdapter
{
    private readonly IDocumentAdapter _innerAdapter;
    private readonly RequestDocumentStore _requestDocumentStore;

    public CapturingDocumentAdapter(IDocumentAdapter innerAdapter, RequestDocumentStore requestDocumentStore)
    {
        ArgumentGuard.NotNull(innerAdapter);
        ArgumentGuard.NotNull(requestDocumentStore);

        _innerAdapter = innerAdapter;
        _requestDocumentStore = requestDocumentStore;
    }

    public object? Convert(Document document)
    {
        _requestDocumentStore.Document = document;
        return _innerAdapter.Convert(document);
    }
}
