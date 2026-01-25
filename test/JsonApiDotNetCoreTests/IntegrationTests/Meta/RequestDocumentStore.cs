using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCoreTests.IntegrationTests.Meta;

internal sealed class RequestDocumentStore
{
    public Document? Document { get; set; }
}
