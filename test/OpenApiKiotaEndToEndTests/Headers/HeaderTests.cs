using FluentAssertions;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options;
using Microsoft.Net.Http.Headers;
using OpenApiKiotaEndToEndTests.Headers.GeneratedCode;
using OpenApiKiotaEndToEndTests.Headers.GeneratedCode.Models;
using OpenApiTests;
using OpenApiTests.Headers;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiKiotaEndToEndTests.Headers;

public sealed class HeaderTests : IClassFixture<IntegrationTestContext<OpenApiStartup<HeaderDbContext>, HeaderDbContext>>, IDisposable
{
    private readonly IntegrationTestContext<OpenApiStartup<HeaderDbContext>, HeaderDbContext> _testContext;
    private readonly TestableHttpClientRequestAdapterFactory _requestAdapterFactory;
    private readonly HeaderFakers _fakers = new();

    public HeaderTests(IntegrationTestContext<OpenApiStartup<HeaderDbContext>, HeaderDbContext> testContext, ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _requestAdapterFactory = new TestableHttpClientRequestAdapterFactory(testOutputHelper);

        testContext.UseController<CountriesController>();
    }

    [Fact]
    public async Task Returns_Location_for_post_resource_request()
    {
        // Arrange
        Country newCountry = _fakers.Country.GenerateOne();

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new HeadersClient(requestAdapter);

        var headerInspector = new HeadersInspectionHandlerOption
        {
            InspectResponseHeaders = true
        };

        var requestBody = new CreateCountryRequestDocument
        {
            Data = new DataInCreateCountryRequest
            {
                Type = CountryResourceType.Countries,
                Attributes = new AttributesInCreateCountryRequest
                {
                    Name = newCountry.Name,
                    Population = newCountry.Population
                }
            }
        };

        // Act
        CountryPrimaryResponseDocument? response =
            await apiClient.Countries.PostAsync(requestBody, configuration => configuration.Options.Add(headerInspector));

        // Assert
        response.ShouldNotBeNull();
        response.Data.ShouldNotBeNull();

        string[] locationHeaderValues = headerInspector.ResponseHeaders.Should().ContainKey(HeaderNames.Location).WhoseValue.ToArray();
        locationHeaderValues.ShouldHaveCount(1);
        locationHeaderValues[0].Should().Be($"/countries/{response.Data.Id}");
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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new HeadersClient(requestAdapter);

        var headerInspector = new HeadersInspectionHandlerOption
        {
            InspectResponseHeaders = true
        };

        // Act
        await apiClient.Countries.HeadAsync(configuration => configuration.Options.Add(headerInspector));

        // Assert
        string[] contentLengthHeaderValues = headerInspector.ResponseHeaders.Should().ContainKey(HeaderNames.ContentLength).WhoseValue.ToArray();
        contentLengthHeaderValues.ShouldHaveCount(1);
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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new HeadersClient(requestAdapter);

        var headerInspector = new HeadersInspectionHandlerOption
        {
            InspectResponseHeaders = true
        };

        // Act
        await apiClient.Countries[existingCountry.Id].HeadAsync(configuration => configuration.Options.Add(headerInspector));

        // Assert
        string[] contentLengthHeaderValues = headerInspector.ResponseHeaders.Should().ContainKey(HeaderNames.ContentLength).WhoseValue.ToArray();
        contentLengthHeaderValues.ShouldHaveCount(1);
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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new HeadersClient(requestAdapter);

        var headerInspector = new HeadersInspectionHandlerOption
        {
            InspectResponseHeaders = true
        };

        // Act
        await apiClient.Countries[existingCountry.Id].Languages.HeadAsync(configuration => configuration.Options.Add(headerInspector));

        // Assert
        string[] contentLengthHeaderValues = headerInspector.ResponseHeaders.Should().ContainKey(HeaderNames.ContentLength).WhoseValue.ToArray();
        contentLengthHeaderValues.ShouldHaveCount(1);
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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new HeadersClient(requestAdapter);

        var headerInspector = new HeadersInspectionHandlerOption
        {
            InspectResponseHeaders = true
        };

        // Act
        await apiClient.Countries[existingCountry.Id].Relationships.Languages.HeadAsync(configuration => configuration.Options.Add(headerInspector));

        // Assert
        string[] contentLengthHeaderValues = headerInspector.ResponseHeaders.Should().ContainKey(HeaderNames.ContentLength).WhoseValue.ToArray();
        contentLengthHeaderValues.ShouldHaveCount(1);
        long.Parse(contentLengthHeaderValues[0]).Should().BeGreaterThan(0);
    }

    public void Dispose()
    {
        _requestAdapterFactory.Dispose();
    }
}
