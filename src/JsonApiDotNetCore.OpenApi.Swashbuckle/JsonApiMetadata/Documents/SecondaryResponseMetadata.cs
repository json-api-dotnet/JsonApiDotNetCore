using System.Net;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata.Documents;

internal sealed class SecondaryResponseMetadata(
    IReadOnlyDictionary<RelationshipAttribute, Type> documentTypesByRelationship, IReadOnlyCollection<HttpStatusCode> successStatusCodes,
    IReadOnlyCollection<HttpStatusCode> errorStatusCodes)
    : NonPrimaryResponseMetadata(documentTypesByRelationship, successStatusCodes, errorStatusCodes);
