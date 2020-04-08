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

            // Add First 422 error
            errors.Add(new Error(HttpStatusCode.UnprocessableEntity) {Title = "Something wrong"});
            Assert.Equal(HttpStatusCode.UnprocessableEntity, new ErrorDocument(errors).GetErrorStatusCode());

            // Add a second 422 error
            errors.Add(new Error(HttpStatusCode.UnprocessableEntity) {Title = "Something else wrong"});
            Assert.Equal(HttpStatusCode.UnprocessableEntity, new ErrorDocument(errors).GetErrorStatusCode());

            // Add 4xx error not 422
            errors.Add(new Error(HttpStatusCode.Unauthorized) {Title = "Unauthorized"});
            Assert.Equal(HttpStatusCode.BadRequest, new ErrorDocument(errors).GetErrorStatusCode());

            // Add 5xx error not 4xx
            errors.Add(new Error(HttpStatusCode.BadGateway) {Title = "Not good"});
            Assert.Equal(HttpStatusCode.InternalServerError, new ErrorDocument(errors).GetErrorStatusCode());
        }
    }
}
