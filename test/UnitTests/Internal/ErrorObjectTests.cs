using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using Xunit;

namespace UnitTests.Internal;

public sealed class ErrorObjectTests
{
    // Formatting below is broken due to Resharper bug at https://youtrack.jetbrains.com/issue/RSRP-494897/Formatter-directive-broken-in-2023.3-EAP7.
    // This no longer works: @formatter:wrap_array_initializer_style wrap_if_long
    [Theory]
    [InlineData(new[]
    {
        HttpStatusCode.UnprocessableEntity
    }, HttpStatusCode.UnprocessableEntity)]
    [InlineData(new[]
    {
        HttpStatusCode.UnprocessableEntity,
        HttpStatusCode.UnprocessableEntity
    }, HttpStatusCode.UnprocessableEntity)]
    [InlineData(new[]
    {
        HttpStatusCode.UnprocessableEntity,
        HttpStatusCode.Unauthorized
    }, HttpStatusCode.BadRequest)]
    [InlineData(new[]
    {
        HttpStatusCode.UnprocessableEntity,
        HttpStatusCode.BadGateway
    }, HttpStatusCode.InternalServerError)]
    public void ErrorDocument_GetErrorStatusCode_IsCorrect(HttpStatusCode[] errorCodes, HttpStatusCode expected)
    {
        // Arrange
        ErrorObject[] errors = errorCodes.Select(code => new ErrorObject(code)).ToArray();

        // Act
        HttpStatusCode status = ErrorObject.GetResponseStatusCode(errors);

        // Assert
        status.Should().Be(expected);
    }
}
