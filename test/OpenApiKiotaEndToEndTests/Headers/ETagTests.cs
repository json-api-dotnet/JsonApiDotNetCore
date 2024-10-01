using System.Net;
using FluentAssertions;
using Microsoft.Kiota.Abstractions;
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

public sealed class ETagTests : IClassFixture<IntegrationTestContext<OpenApiStartup<HeaderDbContext>, HeaderDbContext>>, IDisposable
{
    private readonly IntegrationTestContext<OpenApiStartup<HeaderDbContext>, HeaderDbContext> _testContext;
    private readonly TestableHttpClientRequestAdapterFactory _requestAdapterFactory;
    private readonly HeaderFakers _fakers = new();

    public ETagTests(IntegrationTestContext<OpenApiStartup<HeaderDbContext>, HeaderDbContext> testContext, ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _requestAdapterFactory = new TestableHttpClientRequestAdapterFactory(testOutputHelper);

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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new HeadersClient(requestAdapter);

        var headerInspector = new HeadersInspectionHandlerOption
        {
            InspectResponseHeaders = true
        };

        // Act
        await apiClient.Countries.HeadAsync(configuration => configuration.Options.Add(headerInspector));

        // Assert
        string[] eTagHeaderValues = headerInspector.ResponseHeaders.Should().ContainKey(HeaderNames.ETag).WhoseValue.ToArray();
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

        string[] eTagHeaderValues = headerInspector.ResponseHeaders.Should().ContainKey(HeaderNames.ETag).WhoseValue.ToArray();
        eTagHeaderValues.ShouldHaveCount(1);
        eTagHeaderValues[0].Should().Match("\"*\"");
    }

    [Fact]
    public async Task Returns_no_ETag_for_failed_GET_request()
    {
        // Arrange
        Guid unknownCountryId = Unknown.TypedId.Guid;

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new HeadersClient(requestAdapter);

        var headerInspector = new HeadersInspectionHandlerOption
        {
            InspectResponseHeaders = true
        };

        // Act
        Func<Task> action = async () => await apiClient.Countries[unknownCountryId].GetAsync(configuration => configuration.Options.Add(headerInspector));

        // Assert
        ErrorResponseDocument exception = (await action.Should().ThrowExactlyAsync<ErrorResponseDocument>()).Which;
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.NotFound);
        exception.Message.Should().Be($"Exception of type '{typeof(ErrorResponseDocument).FullName}' was thrown.");
        exception.Errors.ShouldHaveCount(1);

        ErrorObject error = exception.Errors.ElementAt(0);
        error.Status.Should().Be("404");
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'countries' with ID '{unknownCountryId}' does not exist.");

        headerInspector.ResponseHeaders.Should().NotContainKey(HeaderNames.ETag);
    }

    [Fact]
    public async Task Returns_no_ETag_for_POST_request()
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

        headerInspector.ResponseHeaders.Should().NotContainKey(HeaderNames.ETag);
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
        Func<Task> action = async () => await apiClient.Countries.GetAsync(configuration =>
        {
            configuration.Headers.Add(HeaderNames.IfNoneMatch, responseETag);
            configuration.Options.Add(headerInspector);
        });

        // Assert
        ApiException exception = (await action.Should().ThrowExactlyAsync<ApiException>()).Which;
        exception.Message.Should().Be("The server returned an unexpected status code and no error factory is registered for this code: 304");
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.NotModified);

        string[] eTagHeaderValues = headerInspector.ResponseHeaders.Should().ContainKey(HeaderNames.ETag).WhoseValue.ToArray();
        eTagHeaderValues.ShouldHaveCount(1);
        eTagHeaderValues[0].Should().Be(responseETag);
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

        string[] eTagHeaderValues = headerInspector.ResponseHeaders.Should().ContainKey(HeaderNames.ETag).WhoseValue.ToArray();
        eTagHeaderValues.ShouldHaveCount(1);
        eTagHeaderValues[0].Should().Match("\"*\"");
    }

    public void Dispose()
    {
        _requestAdapterFactory.Dispose();
    }
}
