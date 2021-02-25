using System.Linq;
using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using Xunit;

namespace UnitTests.Internal
{
    public sealed class ErrorDocumentTests
    {
        // @formatter:wrap_array_initializer_style wrap_if_long
        [Theory]
        [InlineData(new[] { HttpStatusCode.UnprocessableEntity }, HttpStatusCode.UnprocessableEntity)]
        [InlineData(new[] { HttpStatusCode.UnprocessableEntity, HttpStatusCode.UnprocessableEntity }, HttpStatusCode.UnprocessableEntity)]
        [InlineData(new[] { HttpStatusCode.UnprocessableEntity, HttpStatusCode.Unauthorized }, HttpStatusCode.BadRequest)]
        [InlineData(new[] { HttpStatusCode.UnprocessableEntity, HttpStatusCode.BadGateway }, HttpStatusCode.InternalServerError)]
        // @formatter:wrap_array_initializer_style restore
        public void ErrorDocument_GetErrorStatusCode_IsCorrect(HttpStatusCode[] errorCodes, HttpStatusCode expected)
        {
            // Arrange
            var document = new ErrorDocument(errorCodes.Select(code => new Error(code)));

            // Act
            HttpStatusCode status = document.GetErrorStatusCode();

            // Assert
            status.Should().Be(expected);
        }
    }
}
