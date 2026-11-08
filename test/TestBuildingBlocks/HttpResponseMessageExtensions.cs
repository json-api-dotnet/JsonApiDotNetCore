using System.Net;
using FluentAssertions;
using JetBrains.Annotations;
using Xunit;

namespace TestBuildingBlocks;

[PublicAPI]
public static class HttpResponseMessageExtensions
{
    public static void ShouldHaveStatusCode(this HttpResponseMessage source, HttpStatusCode statusCode)
    {
        // In contrast to the built-in assertion method, this one dumps the response body on mismatch.
        // See https://github.com/fluentassertions/fluentassertions/issues/1811.

        if (source.StatusCode != statusCode)
        {
            string responseText = source.Content.ReadAsStringAsync(TestContext.Current.CancellationToken).Result;
            source.StatusCode.Should().Be(statusCode, string.IsNullOrEmpty(responseText) ? null : $"response body returned was:\n{responseText}");
        }
    }
}
