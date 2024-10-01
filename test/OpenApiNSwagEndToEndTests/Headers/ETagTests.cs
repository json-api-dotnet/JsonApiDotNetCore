using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.OpenApi.Client.NSwag;
using Microsoft.Net.Http.Headers;
using OpenApiNSwagEndToEndTests.Headers.GeneratedCode;
using OpenApiTests;
using OpenApiTests.Headers;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiNSwagEndToEndTests.Headers;

public sealed class ETagTests : IClassFixture<IntegrationTestContext<OpenApiStartup<HeaderDbContext>, HeaderDbContext>>, IDisposable
{
    private readonly IntegrationTestContext<OpenApiStartup<HeaderDbContext>, HeaderDbContext> _testContext;
    private readonly XUnitLogHttpMessageHandler _logHttpMessageHandler;
    private readonly HeaderFakers _fakers = new();

    public ETagTests(IntegrationTestContext<OpenApiStartup<HeaderDbContext>, HeaderDbContext> testContext, ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _logHttpMessageHandler = new XUnitLogHttpMessageHandler(testOutputHelper);

        testContext.UseController<CountriesController>();
    }

    [Fact]
    public async Task Returns_ETag_for_HEAD_request()
    {
        // Arrange
        List<Country> countries = _fakers.Country.GenerateList(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Country>();
            dbContext.Countries.AddRange(countries);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new HeadersClient(httpClient);

        // Act
        ApiResponse response = await ApiResponse.TranslateAsync(async () => await apiClient.HeadCountryCollectionAsync());

        // Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);

        string[] eTagHeaderValues = response.Headers.Should().ContainKey(HeaderNames.ETag).WhoseValue.ToArray();
        eTagHeaderValues.ShouldHaveCount(1);
        eTagHeaderValues[0].Should().Match("\"*\"");
    }

    [Fact]
    public async Task Returns_ETag_for_GET_request()
    {
        // Arrange
        List<Country> countries = _fakers.Country.GenerateList(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Country>();
            dbContext.Countries.AddRange(countries);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new HeadersClient(httpClient);

        // Act
        ApiResponse<CountryCollectionResponseDocument?> response = await ApiResponse.TranslateAsync(async () => await apiClient.GetCountryCollectionAsync());

        // Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);

        string[] eTagHeaderValues = response.Headers.Should().ContainKey(HeaderNames.ETag).WhoseValue.ToArray();
        eTagHeaderValues.ShouldHaveCount(1);
        eTagHeaderValues[0].Should().Match("\"*\"");

        response.Result.ShouldNotBeNull();
    }

    [Fact]
    public async Task Returns_no_ETag_for_failed_GET_request()
    {
        // Arrange
        Guid unknownCountryId = Unknown.TypedId.Guid;

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new HeadersClient(httpClient);

        // Act
        Func<Task> action = async () => await ApiResponse.TranslateAsync(async () => await apiClient.GetCountryAsync(unknownCountryId));

        // Assert
        ApiException<ErrorResponseDocument> exception = (await action.Should().ThrowExactlyAsync<ApiException<ErrorResponseDocument>>()).Which;
        exception.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        exception.Message.Should().Be("HTTP 404: The country does not exist.");
        exception.Headers.Should().NotContainKey(HeaderNames.ETag);
        exception.Result.Errors.ShouldHaveCount(1);

        ErrorObject error = exception.Result.Errors.ElementAt(0);
        error.Status.Should().Be("404");
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'countries' with ID '{unknownCountryId}' does not exist.");
    }

    [Fact]
    public async Task Returns_no_ETag_for_POST_request()
    {
        // Arrange
        Country newCountry = _fakers.Country.GenerateOne();

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new HeadersClient(httpClient);

        var requestBody = new CreateCountryRequestDocument
        {
            Data = new DataInCreateCountryRequest
            {
                Attributes = new AttributesInCreateCountryRequest
                {
                    Name = newCountry.Name,
                    Population = newCountry.Population
                }
            }
        };

        // Act
        ApiResponse<CountryPrimaryResponseDocument?> response = await ApiResponse.TranslateAsync(async () => await apiClient.PostCountryAsync(requestBody));

        // Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.Created);

        response.Headers.Should().NotContainKey(HeaderNames.ETag);

        response.Result.ShouldNotBeNull();
    }

    [Fact]
    public async Task Returns_NotModified_for_matching_ETag()
    {
        // Arrange
        List<Country> countries = _fakers.Country.GenerateList(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Country>();
            dbContext.Countries.AddRange(countries);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new HeadersClient(httpClient);

        ApiResponse<CountryCollectionResponseDocument?> response1 = await ApiResponse.TranslateAsync(async () => await apiClient.GetCountryCollectionAsync());

        string responseETag = response1.Headers[HeaderNames.ETag].Single();

        // Act
        ApiResponse<CountryCollectionResponseDocument?> response2 =
            await ApiResponse.TranslateAsync(async () => await apiClient.GetCountryCollectionAsync(null, responseETag));

        // Assert
        response2.StatusCode.Should().Be((int)HttpStatusCode.NotModified);

        string[] eTagHeaderValues = response2.Headers.Should().ContainKey(HeaderNames.ETag).WhoseValue.ToArray();
        eTagHeaderValues.ShouldHaveCount(1);
        eTagHeaderValues[0].Should().Be(responseETag);

        response2.Result.Should().BeNull();
    }

    [Fact]
    public async Task Returns_content_for_mismatching_ETag()
    {
        // Arrange
        List<Country> countries = _fakers.Country.GenerateList(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Country>();
            dbContext.Countries.AddRange(countries);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new HeadersClient(httpClient);

        // Act
        ApiResponse<CountryCollectionResponseDocument?> response =
            await ApiResponse.TranslateAsync(async () => await apiClient.GetCountryCollectionAsync(null, "\"Not-a-matching-value\""));

        // Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);

        string[] eTagHeaderValues = response.Headers.Should().ContainKey(HeaderNames.ETag).WhoseValue.ToArray();
        eTagHeaderValues.ShouldHaveCount(1);
        eTagHeaderValues[0].Should().Match("\"*\"");

        response.Result.ShouldNotBeNull();
    }

    public void Dispose()
    {
        _logHttpMessageHandler.Dispose();
    }
}
