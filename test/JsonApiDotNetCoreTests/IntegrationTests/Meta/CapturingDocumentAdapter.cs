using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Serialization.Request.Adapters;

namespace JsonApiDotNetCoreTests.IntegrationTests.Meta;

public sealed class CapturingDocumentAdapter : IDocumentAdapter
{
    private readonly IDocumentAdapter _innerAdapter;
    private readonly RequestDocumentStore _requestDocumentStore;

    public CapturingDocumentAdapter(IDocumentAdapter innerAdapter, RequestDocumentStore requestDocumentStore)
    {
        ArgumentNullException.ThrowIfNull(innerAdapter);
        ArgumentNullException.ThrowIfNull(requestDocumentStore);

        _innerAdapter = innerAdapter;
        _requestDocumentStore = requestDocumentStore;
    }

    public object? Convert(Document document)
    {
        _requestDocumentStore.Document = document;
        return _innerAdapter.Convert(document);
    }
}


