using System.Net;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata.Documents;

internal sealed class EmptyRelationshipResponseMetadata : IJsonApiResponseMetadata
{
    public IReadOnlyCollection<RelationshipAttribute> Relationships { get; }

    public IReadOnlyCollection<HttpStatusCode> SuccessStatusCodes { get; }
    public IReadOnlyCollection<HttpStatusCode> ErrorStatusCodes { get; }

    public EmptyRelationshipResponseMetadata(IReadOnlyCollection<RelationshipAttribute> relationships, IReadOnlyCollection<HttpStatusCode> successStatusCodes,
        IReadOnlyCollection<HttpStatusCode> errorStatusCodes)
    {
        ArgumentNullException.ThrowIfNull(relationships);
        ArgumentNullException.ThrowIfNull(successStatusCodes);
        ArgumentNullException.ThrowIfNull(errorStatusCodes);

        Relationships = relationships;
        SuccessStatusCodes = successStatusCodes;
        ErrorStatusCodes = errorStatusCodes;
    }
}
