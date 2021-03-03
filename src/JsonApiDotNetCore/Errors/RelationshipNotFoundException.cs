using System.Net;
using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The error that is thrown when a relationship does not exist.
    /// </summary>
    [PublicAPI]
    public sealed class RelationshipNotFoundException : JsonApiException
    {
        public RelationshipNotFoundException(string relationshipName, string resourceType)
            : base(new Error(HttpStatusCode.NotFound)
            {
                Title = "The requested relationship does not exist.",
                Detail = $"Resource of type '{resourceType}' does not contain a relationship named '{relationshipName}'."
            })
        {
        }
    }
}
