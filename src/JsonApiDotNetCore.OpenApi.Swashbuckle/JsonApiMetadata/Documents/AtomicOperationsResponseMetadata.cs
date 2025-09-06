using System.Net;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Documents;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata.Documents;

internal sealed class AtomicOperationsResponseMetadata : IJsonApiResponseMetadata
{
    public static AtomicOperationsResponseMetadata Instance { get; } = new();

    public Type DocumentType => typeof(OperationsResponseDocument);

    public IReadOnlyCollection<HttpStatusCode> SuccessStatusCodes { get; } =
    [
        HttpStatusCode.OK,
        HttpStatusCode.NoContent
    ];

    public IReadOnlyCollection<HttpStatusCode> ErrorStatusCodes { get; } =
    [
        HttpStatusCode.BadRequest,
        // Forbidden doesn't depend on whether ClientIdGeneration is enabled, because it is also used when an operation is not accessible.
        HttpStatusCode.Forbidden,
        HttpStatusCode.NotFound,
        HttpStatusCode.Conflict,
        HttpStatusCode.UnprocessableEntity
    ];

    private AtomicOperationsResponseMetadata()
    {
    }
}
