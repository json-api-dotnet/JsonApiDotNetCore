using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCoreTests.IntegrationTests.ContentNegotiation.CustomExtensions;

internal sealed class RequestDocumentStore
{
    public Document? Document { get; set; }
}
