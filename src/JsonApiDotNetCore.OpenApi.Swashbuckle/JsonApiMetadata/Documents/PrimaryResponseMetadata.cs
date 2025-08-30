using System.Net;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata.Documents;

internal sealed class PrimaryResponseMetadata : IJsonApiResponseMetadata
{
    public Type? DocumentType { get; }
    public IReadOnlyCollection<HttpStatusCode> SuccessStatusCodes { get; }
    public IReadOnlyCollection<HttpStatusCode> ErrorStatusCodes { get; }

    public PrimaryResponseMetadata(Type? documentType, IReadOnlyCollection<HttpStatusCode> successStatusCodes,
        IReadOnlyCollection<HttpStatusCode> errorStatusCodes)
    {
        ArgumentNullException.ThrowIfNull(successStatusCodes);
        ArgumentNullException.ThrowIfNull(errorStatusCodes);

        DocumentType = documentType;
        SuccessStatusCodes = successStatusCodes;
        ErrorStatusCodes = errorStatusCodes;
    }
}
