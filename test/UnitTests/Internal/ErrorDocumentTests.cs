using System.Collections.Generic;
using System.Net;
using JsonApiDotNetCore.Models.JsonApiDocuments;
using Xunit;

namespace UnitTests.Internal
{
    public sealed class ErrorDocumentTests
    {
        [Fact]
        public void Can_GetStatusCode()
        {
            List<Error> errors = new List<Error>();
            var document = new ErrorDocument(errors);

            // Add First 422 error
            errors.Add(new Error(HttpStatusCode.UnprocessableEntity, "Something wrong"));
            Assert.Equal(HttpStatusCode.UnprocessableEntity, document.GetErrorStatusCode());

            // Add a second 422 error
            errors.Add(new Error(HttpStatusCode.UnprocessableEntity, "Something else wrong"));
            Assert.Equal(HttpStatusCode.UnprocessableEntity, document.GetErrorStatusCode());

            // Add 4xx error not 422
            errors.Add(new Error(HttpStatusCode.Unauthorized, "Unauthorized"));
            Assert.Equal(HttpStatusCode.BadRequest, document.GetErrorStatusCode());

            // Add 5xx error not 4xx
            errors.Add(new Error(HttpStatusCode.BadGateway, "Not good"));
            Assert.Equal(HttpStatusCode.InternalServerError, document.GetErrorStatusCode());
        }
    }
}
