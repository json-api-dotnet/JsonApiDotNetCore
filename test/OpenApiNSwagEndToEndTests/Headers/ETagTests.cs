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

public sealed class ETagTests : IClassFixture<IntegrationTestContext<OpenApiStartup<HeaderDbContext>, HeaderDbContext>>
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
        List<Country> countries = _fakers.Country.Generate(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Country>();
            dbContext.Countries.AddRange(countries);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new HeadersClient(httpClient);

        // Act
        ApiResponse response = await ApiResponse.TranslateAsync(() => apiClient.HeadCountryCollectionAsync(null, null));

        // Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);

        response.Headers.Should().ContainKey(HeaderNames.ETag).WhoseValue.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Returns_ETag_for_GET_request()
    {
        // Arrange
        List<Country> countries = _fakers.Country.Generate(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Country>();
            dbContext.Countries.AddRange(countries);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new HeadersClient(httpClient);

        // Act
        ApiResponse<CountryCollectionResponseDocument?> response = await ApiResponse.TranslateAsync(() => apiClient.GetCountryCollectionAsync(null, null));

        // Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);

        response.Headers.Should().ContainKey(HeaderNames.ETag).WhoseValue.Should().NotBeNullOrEmpty();

        response.Result.ShouldNotBeNull();
    }

    [Fact]
    public async Task Returns_no_ETag_for_failed_GET_request()
    {
        // Arrange
        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new HeadersClient(httpClient);

        // Act
        Func<Task<ApiResponse<CountryPrimaryResponseDocument?>>> action = () =>
            ApiResponse.TranslateAsync(() => apiClient.GetCountryAsync(Unknown.StringId.For<Country, Guid>(), null, null));

        // Assert
        ApiException<ErrorResponseDocument> exception = (await action.Should().ThrowExactlyAsync<ApiException<ErrorResponseDocument>>()).Which;
        exception.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        exception.Headers.Should().NotContainKey(HeaderNames.ETag);
    }

    [Fact]
    public async Task Returns_no_ETag_for_POST_request()
    {
        // Arrange
        Country newCountry = _fakers.Country.Generate();

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new HeadersClient(httpClient);

        // Act
        ApiResponse<CountryPrimaryResponseDocument?> response = await ApiResponse.TranslateAsync(() => apiClient.PostCountryAsync(null,
            new CountryPostRequestDocument
            {
                Data = new CountryDataInPostRequest
                {
                    Attributes = new CountryAttributesInPostRequest
                    {
                        Name = newCountry.Name,
                        Population = newCountry.Population
                    }
                }
            }));

        // Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.Created);

        response.Headers.Should().NotContainKey(HeaderNames.ETag);

        response.Result.ShouldNotBeNull();
    }

    [Fact]
    public async Task Returns_NotModified_for_matching_ETag()
    {
        // Arrange
        List<Country> countries = _fakers.Country.Generate(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Country>();
            dbContext.Countries.AddRange(countries);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new HeadersClient(httpClient);

        ApiResponse<CountryCollectionResponseDocument?> response1 = await ApiResponse.TranslateAsync(() => apiClient.GetCountryCollectionAsync(null, null));

        string responseETag = response1.Headers[HeaderNames.ETag].Single();

        // Act
        ApiResponse<CountryCollectionResponseDocument?> response2 =
            await ApiResponse.TranslateAsync(() => apiClient.GetCountryCollectionAsync(null, responseETag));

        // Assert
        response2.StatusCode.Should().Be((int)HttpStatusCode.NotModified);

        response2.Headers.Should().ContainKey(HeaderNames.ETag).WhoseValue.Should().Equal([responseETag]);

        response2.Result.Should().BeNull();
    }

    [Fact]
    public async Task Returns_content_for_mismatching_ETag()
    {
        // Arrange
        List<Country> countries = _fakers.Country.Generate(2);

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
            await ApiResponse.TranslateAsync(() => apiClient.GetCountryCollectionAsync(null, "\"Not-a-matching-value\""));

        // Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);

        response.Headers.Should().ContainKey(HeaderNames.ETag).WhoseValue.Should().NotBeNullOrEmpty();

        response.Result.ShouldNotBeNull();
    }
}
