using System.Net;
using JsonApiDotNetCore.Models.JsonApiDocuments;

namespace JsonApiDotNetCore.Exceptions
{ 
    public sealed class IdPropertyNotFoundException : JsonApiException
    {
        public IdPropertyNotFoundException(string resourceName)
            : base(new Error(HttpStatusCode.BadRequest)
            {
                Title = "The resource does not have a key field defined.",
                Detail = $"The resource '{resourceName}' does not specify an Id proeprty (By convention 'Id' or specified with [Id] attribute)'."
            })
        { }
    }
}
