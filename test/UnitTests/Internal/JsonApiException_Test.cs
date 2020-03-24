using JsonApiDotNetCore.Internal;
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
            errors.Add(new Error(422, "Something wrong"));
            Assert.Equal(422, exception.GetStatusCode());

            // Add a second 422 error
            errors.Add(new Error(422, "Something else wrong"));
            Assert.Equal(422, exception.GetStatusCode());

            // Add 4xx error not 422
            errors.Add(new Error(401, "Unauthorized"));
            Assert.Equal(400, exception.GetStatusCode());

            // Add 5xx error not 4xx
            errors.Add(new Error(502, "Not good"));
            Assert.Equal(500, exception.GetStatusCode());
        }
    }
}
