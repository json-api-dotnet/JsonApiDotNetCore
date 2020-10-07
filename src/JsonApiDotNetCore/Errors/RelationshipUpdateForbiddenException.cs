using System.Net;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The error that is thrown when a request is received that contains an unsupported HTTP verb.
    /// </summary>
    public sealed class RelationshipUpdateForbiddenException : JsonApiException
    {
        public RelationshipUpdateForbiddenException(string toOneRelationship)
            : base(new Error(HttpStatusCode.Forbidden)
            {
                Title = "The request to update the relationship is forbidden.",
                Detail = $"Relationship {toOneRelationship} is not a to-many relationship."
            }) { }
    }
}
