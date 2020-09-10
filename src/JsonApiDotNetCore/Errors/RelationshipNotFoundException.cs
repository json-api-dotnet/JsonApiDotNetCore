using System.Net;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The error that is thrown when a relationship does not exist.
    /// </summary>
    public sealed class RelationshipNotFoundException : JsonApiException
    {
        public RelationshipNotFoundException(string relationshipName, string containingResourceName) : base(new Error(HttpStatusCode.NotFound)
        {
            Title = "The requested relationship does not exist.",
            Detail = $"The resource '{containingResourceName}' does not contain a relationship named '{relationshipName}'."
        })
        {
        }
    }
}
