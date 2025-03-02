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

public sealed class HeaderTests : IClassFixture<IntegrationTestContext<OpenApiStartup<HeaderDbContext>, HeaderDbContext>>, IDisposable
{
    private readonly IntegrationTestContext<OpenApiStartup<HeaderDbContext>, HeaderDbContext> _testContext;
    private readonly XUnitLogHttpMessageHandler _logHttpMessageHandler;
    private readonly HeaderFakers _fakers = new();

    public HeaderTests(IntegrationTestContext<OpenApiStartup<HeaderDbContext>, HeaderDbContext> testContext, ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _logHttpMessageHandler = new XUnitLogHttpMessageHandler(testOutputHelper);

        testContext.UseController<CountriesController>();
    }

    [Fact]
    public async Task Returns_Location_for_post_resource_request()
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

        response.Result.ShouldNotBeNull();

        string[] locationHeaderValues = response.Headers.Should().ContainKey(HeaderNames.Location).WhoseValue.ToArray();
        locationHeaderValues.Should().HaveCount(1);
        locationHeaderValues[0].Should().Be($"/countries/{response.Result.Data.Id}");
    }

    [Fact]
    public async Task Returns_ContentLength_for_head_primary_resources_request()
    {
        // Arrange
        Country existingCountry = _fakers.Country.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Country>();
            dbContext.Countries.Add(existingCountry);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new HeadersClient(httpClient);

        // Act
        ApiResponse response = await ApiResponse.TranslateAsync(async () => await apiClient.HeadCountryCollectionAsync());

        // Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);

        string[] contentLengthHeaderValues = response.Headers.Should().ContainKey(HeaderNames.ContentLength).WhoseValue.ToArray();
        contentLengthHeaderValues.Should().HaveCount(1);
        long.Parse(contentLengthHeaderValues[0]).Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Returns_ContentLength_for_head_primary_resource_request()
    {
        // Arrange
        Country existingCountry = _fakers.Country.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Countries.Add(existingCountry);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new HeadersClient(httpClient);

        // Act
        ApiResponse response = await ApiResponse.TranslateAsync(async () => await apiClient.HeadCountryAsync(existingCountry.Id));

        // Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);

        string[] contentLengthHeaderValues = response.Headers.Should().ContainKey(HeaderNames.ContentLength).WhoseValue.ToArray();
        contentLengthHeaderValues.Should().HaveCount(1);
        long.Parse(contentLengthHeaderValues[0]).Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Returns_ContentLength_for_head_secondary_resource_request()
    {
        // Arrange
        Country existingCountry = _fakers.Country.GenerateOne();
        existingCountry.Languages = _fakers.Language.GenerateSet(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Countries.Add(existingCountry);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new HeadersClient(httpClient);

        // Act
        ApiResponse response = await ApiResponse.TranslateAsync(async () => await apiClient.HeadCountryLanguagesAsync(existingCountry.Id));

        // Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);

        string[] contentLengthHeaderValues = response.Headers.Should().ContainKey(HeaderNames.ContentLength).WhoseValue.ToArray();
        contentLengthHeaderValues.Should().HaveCount(1);
        long.Parse(contentLengthHeaderValues[0]).Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Returns_ContentLength_for_head_relationship_request()
    {
        // Arrange
        Country existingCountry = _fakers.Country.GenerateOne();
        existingCountry.Languages = _fakers.Language.GenerateSet(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Countries.Add(existingCountry);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new HeadersClient(httpClient);

        // Act
        ApiResponse response = await ApiResponse.TranslateAsync(async () => await apiClient.HeadCountryLanguagesRelationshipAsync(existingCountry.Id));

        // Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);

        string[] contentLengthHeaderValues = response.Headers.Should().ContainKey(HeaderNames.ContentLength).WhoseValue.ToArray();
        contentLengthHeaderValues.Should().HaveCount(1);
        long.Parse(contentLengthHeaderValues[0]).Should().BeGreaterThan(0);
    }

    public void Dispose()
    {
        _logHttpMessageHandler.Dispose();
    }
}
