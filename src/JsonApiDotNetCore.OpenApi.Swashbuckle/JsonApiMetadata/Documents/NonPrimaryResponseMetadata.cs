using System.Net;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata.Documents;

internal class NonPrimaryResponseMetadata : IJsonApiResponseMetadata
{
    public IReadOnlyDictionary<RelationshipAttribute, Type> DocumentTypesByRelationship { get; }

    public IReadOnlyCollection<HttpStatusCode> SuccessStatusCodes { get; }
    public IReadOnlyCollection<HttpStatusCode> ErrorStatusCodes { get; }

    protected NonPrimaryResponseMetadata(IReadOnlyDictionary<RelationshipAttribute, Type> documentTypesByRelationship,
        IReadOnlyCollection<HttpStatusCode> successStatusCodes, IReadOnlyCollection<HttpStatusCode> errorStatusCodes)
    {
        ArgumentNullException.ThrowIfNull(documentTypesByRelationship);
        ArgumentNullException.ThrowIfNull(successStatusCodes);
        ArgumentNullException.ThrowIfNull(errorStatusCodes);

        DocumentTypesByRelationship = documentTypesByRelationship;
        SuccessStatusCodes = successStatusCodes;
        ErrorStatusCodes = errorStatusCodes;
    }
}
