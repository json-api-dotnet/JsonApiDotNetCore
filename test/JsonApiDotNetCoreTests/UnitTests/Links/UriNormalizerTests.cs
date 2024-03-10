using FluentAssertions;
using JsonApiDotNetCore.Serialization.Response;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.Links;

public sealed class UriNormalizerTests
{
    [Theory]
    [InlineData("some/path", "http://localhost")]
    [InlineData("some?version=1", "http://localhost")]
    public void Keeps_relative_URL_relative(string sourceUrl, string requestUrl)
    {
        // Arrange
        var normalizer = new UriNormalizer();

        // Act
        string result = normalizer.Normalize(sourceUrl, true, new Uri(requestUrl));

        // Assert
        result.Should().Be(sourceUrl);
    }

    [Theory]
    [InlineData("some/path", "http://localhost", "http://localhost/some/path")]
    [InlineData("some/path", "https://api-server.com", "https://api-server.com/some/path")]
    [InlineData("some/path", "https://user:pass@api-server.com:9999", "https://user:pass@api-server.com:9999/some/path")]
    [InlineData("some/path", "http://localhost/api/articles?debug=true#anchor", "http://localhost/some/path")]
    [InlineData("some?version=1", "http://localhost/api/articles/1?debug=true#anchor", "http://localhost/some?version=1")]
    public void Makes_relative_URL_absolute(string sourceUrl, string requestUrl, string expected)
    {
        // Arrange
        var normalizer = new UriNormalizer();

        // Act
        string result = normalizer.Normalize(sourceUrl, false, new Uri(requestUrl));

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("http://localhost/some/path", "http://api-server.com")]
    [InlineData("http://localhost/some/path", "https://localhost")]
    [InlineData("http://localhost:8080/some/path", "http://localhost")]
    [InlineData("http://user:pass@localhost/some/path?version=1", "http://localhost")]
    [InlineData("http://user:pass@localhost/some/path?version=1", "http://USER:PASS@localhost")]
    public void Keeps_absolute_URL_absolute(string sourceUrl, string requestUrl)
    {
        // Arrange
        var normalizer = new UriNormalizer();

        // Act
        string result = normalizer.Normalize(sourceUrl, true, new Uri(requestUrl));

        // Assert
        result.Should().Be(sourceUrl);
    }

    [Theory]
    [InlineData("http://localhost/some/path", "http://localhost/api/articles/1", "some/path")]
    [InlineData("http://api-server.com/some/path", "http://api-server.com/api/articles/1", "some/path")]
    [InlineData("https://localhost/some/path", "https://localhost/api/articles/1", "some/path")]
    [InlineData("https://localhost:443/some/path", "https://localhost/api/articles/1", "some/path")]
    [InlineData("https://localhost/some/path", "https://localhost:443/api/articles/1", "some/path")]
    [InlineData("HTTPS://LOCALHOST/some/path", "https://localhost:443/api/articles/1", "some/path")]
    public void Makes_absolute_URL_relative(string sourceUrl, string requestUrl, string expected)
    {
        // Arrange
        var normalizer = new UriNormalizer();

        // Act
        string result = normalizer.Normalize(sourceUrl, true, new Uri(requestUrl));

        // Assert
        result.Should().Be(expected);
    }
}
