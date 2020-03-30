using System.Net;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models.JsonApiDocuments;
using Xunit;

namespace UnitTests.Internal
{
    public sealed class JsonApiException_Test
    {
        [Fact]
        public void Can_GetStatusCode()
        {
            var errors = new ErrorCollection();
            var exception = new JsonApiException(errors);

            // Add First 422 error
            errors.Add(new Error(HttpStatusCode.UnprocessableEntity, "Something wrong"));
            Assert.Equal(HttpStatusCode.UnprocessableEntity, exception.GetStatusCode());

            // Add a second 422 error
            errors.Add(new Error(HttpStatusCode.UnprocessableEntity, "Something else wrong"));
            Assert.Equal(HttpStatusCode.UnprocessableEntity, exception.GetStatusCode());

            // Add 4xx error not 422
            errors.Add(new Error(HttpStatusCode.Unauthorized, "Unauthorized"));
            Assert.Equal(HttpStatusCode.BadRequest, exception.GetStatusCode());

            // Add 5xx error not 4xx
            errors.Add(new Error(HttpStatusCode.BadGateway, "Not good"));
            Assert.Equal(HttpStatusCode.InternalServerError, exception.GetStatusCode());
        }
    }
}
