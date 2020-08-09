using System.Net;
using JsonApiDotNetCore.Models.JsonApiDocuments;

namespace JsonApiDotNetCore.Exceptions
{
    public sealed class DuplicateIdPropertyException : JsonApiException
    {
        public DuplicateIdPropertyException(string resourceType, string idPropertyName, string duplicateIdPropertyName)
            : base(new Error(HttpStatusCode.BadRequest)
            {
                Title = "More than one id field is defined.",
                Detail = $"The resource '{resourceType}' has multiple id properties defined '{idPropertyName}', '{duplicateIdPropertyName}'."
            })
        { }
    }
}
