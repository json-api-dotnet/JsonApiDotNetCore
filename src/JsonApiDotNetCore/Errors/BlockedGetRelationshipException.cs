using System.Net;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors;

[PublicAPI]
public sealed class BlockedGetRelationshipException(RelationshipAttribute relationship)
    : JsonApiException(new ErrorObject(HttpStatusCode.Forbidden)
    {
        Title = "The requested endpoint is not accessible.",
        Detail = $"Retrieving the relationship '{relationship.PublicName}' of type '{relationship.LeftType}' is not allowed."
    });
