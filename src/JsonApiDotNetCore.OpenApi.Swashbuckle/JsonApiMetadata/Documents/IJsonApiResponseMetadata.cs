using System.Net;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata.Documents;

internal interface IJsonApiResponseMetadata
{
    IReadOnlyCollection<HttpStatusCode> SuccessStatusCodes { get; }
    IReadOnlyCollection<HttpStatusCode> ErrorStatusCodes { get; }
}
