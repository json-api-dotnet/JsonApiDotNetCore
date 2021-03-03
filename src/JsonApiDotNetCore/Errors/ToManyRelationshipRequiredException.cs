using System.Net;
using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The error that is thrown when an attempt is made to update a to-one relationship from a to-many relationship endpoint.
    /// </summary>
    [PublicAPI]
    public sealed class ToManyRelationshipRequiredException : JsonApiException
    {
        public ToManyRelationshipRequiredException(string relationshipName)
            : base(new Error(HttpStatusCode.Forbidden)
            {
                Title = "Only to-many relationships can be updated through this endpoint.",
                Detail = $"Relationship '{relationshipName}' must be a to-many relationship."
            })
        {
        }
    }
}
