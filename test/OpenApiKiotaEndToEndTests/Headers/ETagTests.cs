using System.Net;
using FluentAssertions;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options;
using Microsoft.Net.Http.Headers;
using OpenApiNSwagEndToEndTests.Headers.GeneratedCode;
using OpenApiNSwagEndToEndTests.Headers.GeneratedCode.Models;
using OpenApiTests;
using OpenApiTests.Headers;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;

namespace OpenApiKiotaEndToEndTests.Headers;

public sealed class ETagTests : IClassFixture<IntegrationTestContext<OpenApiStartup<HeadersDbContext>, HeadersDbContext>>
{
    private readonly IntegrationTestContext<OpenApiStartup<HeadersDbContext>, HeadersDbContext> _testContext;
    private readonly TestableHttpClientRequestAdapterFactory _requestAdapterFactory;
    private readonly HeaderFakers _fakers = new();

    public ETagTests(IntegrationTestContext<OpenApiStartup<HeadersDbContext>, HeadersDbContext> testContext, ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _requestAdapterFactory = new TestableHttpClientRequestAdapterFactory(testOutputHelper);

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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new HeadersClient(requestAdapter);

        var headerInspector = new HeadersInspectionHandlerOption
        {
            InspectResponseHeaders = true
        };

        // Act
        Stream? response = await apiClient.Countries.HeadAsync(configuration => configuration.Options.Add(headerInspector));

        // Assert
        response.Should().BeNullOrEmpty();

        headerInspector.ResponseHeaders.Should().ContainKey(HeaderNames.ETag).WhoseValue.Should().NotBeNullOrEmpty();
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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new HeadersClient(requestAdapter);

        var headerInspector = new HeadersInspectionHandlerOption
        {
            InspectResponseHeaders = true
        };

        // Act
        CountryCollectionResponseDocument? response = await apiClient.Countries.GetAsync(configuration => configuration.Options.Add(headerInspector));

        // Assert
        response.ShouldNotBeNull();

        headerInspector.ResponseHeaders.Should().ContainKey(HeaderNames.ETag).WhoseValue.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Returns_no_ETag_for_failed_GET_request()
    {
        // Arrange
        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new HeadersClient(requestAdapter);

        var headerInspector = new HeadersInspectionHandlerOption
        {
            InspectResponseHeaders = true
        };

        // Act
        Func<Task<CountryPrimaryResponseDocument?>> action = () => apiClient.Countries[Unknown.StringId.For<Country, Guid>()]
            .GetAsync(configuration => configuration.Options.Add(headerInspector));

        // Assert
        ErrorResponseDocument exception = (await action.Should().ThrowExactlyAsync<ErrorResponseDocument>()).Which;
        exception.Errors.ShouldHaveCount(1);
        exception.Errors[0].Status.Should().Be(((int)HttpStatusCode.NotFound).ToString());

        headerInspector.ResponseHeaders.Should().NotContainKey(HeaderNames.ETag);
    }

    [Fact]
    public async Task Returns_no_ETag_for_POST_request()
    {
        // Arrange
        Country newCountry = _fakers.Country.Generate();

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new HeadersClient(requestAdapter);

        var headerInspector = new HeadersInspectionHandlerOption
        {
            InspectResponseHeaders = true
        };

        // Act
        CountryPrimaryResponseDocument? response = await apiClient.Countries.PostAsync(new CountryPostRequestDocument
        {
            Data = new CountryDataInPostRequest
            {
                Type = CountryResourceType.Countries,
                Attributes = new CountryAttributesInPostRequest
                {
                    Name = newCountry.Name,
                    Population = newCountry.Population
                }
            }
        }, configuration => configuration.Options.Add(headerInspector));

        // Assert
        response.ShouldNotBeNull();

        headerInspector.ResponseHeaders.Should().NotContainKey(HeaderNames.ETag);
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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new HeadersClient(requestAdapter);

        var headerInspector = new HeadersInspectionHandlerOption
        {
            InspectResponseHeaders = true
        };

        _ = await apiClient.Countries.GetAsync(configuration => configuration.Options.Add(headerInspector));

        string responseETag = headerInspector.ResponseHeaders[HeaderNames.ETag].Single();
        headerInspector.ResponseHeaders.Clear();

        // Act
        Func<Task<CountryCollectionResponseDocument?>> action = () => apiClient.Countries.GetAsync(configuration =>
        {
            configuration.Headers.Add(HeaderNames.IfNoneMatch, responseETag);
            configuration.Options.Add(headerInspector);
        });

        // Assert
        ApiException exception = (await action.Should().ThrowExactlyAsync<ApiException>()).Which;
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.NotModified);

        headerInspector.ResponseHeaders.Should().ContainKey(HeaderNames.ETag).WhoseValue.Should().Equal([responseETag]);
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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new HeadersClient(requestAdapter);

        var headerInspector = new HeadersInspectionHandlerOption
        {
            InspectResponseHeaders = true
        };

        // Act
        CountryCollectionResponseDocument? response = await apiClient.Countries.GetAsync(configuration =>
        {
            configuration.Headers.Add(HeaderNames.IfNoneMatch, "\"Not-a-matching-value\"");
            configuration.Options.Add(headerInspector);
        });

        // Assert
        response.ShouldNotBeNull();

        headerInspector.ResponseHeaders.Should().ContainKey(HeaderNames.ETag).WhoseValue.Should().NotBeNullOrEmpty();
    }
}
