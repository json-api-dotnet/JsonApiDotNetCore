using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.OpenApi.Client;
using Microsoft.Net.Http.Headers;
using OpenApiEndToEndTests.Headers.GeneratedCode;
using OpenApiTests;
using OpenApiTests.Headers;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiEndToEndTests.Headers;

public sealed class ETagTests : IClassFixture<IntegrationTestContext<OpenApiStartup<HeadersDbContext>, HeadersDbContext>>
{
    private readonly IntegrationTestContext<OpenApiStartup<HeadersDbContext>, HeadersDbContext> _testContext;
    private readonly HeaderFakers _fakers = new();

    public ETagTests(IntegrationTestContext<OpenApiStartup<HeadersDbContext>, HeadersDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<CountriesController>();
    }

    [Fact]
    public async Task Get_should_return_etag()
    {
        // Arrange
        Country country = _fakers.Country.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Country>();
            dbContext.Countries.Add(country);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        var apiClient = new HeadersClient(httpClient);

        // Act
        ApiResponse<CountryPrimaryResponseDocument?> response = await ApiResponse.TranslateAsync(() => apiClient.GetCountryAsync(country.StringId!));

        // Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.Headers.Should().ContainKey(HeaderNames.ETag);
    }

    [Fact]
    public async Task Getting_twice_unmodified_resource_should_return_304()
    {
        // Arrange
        Country country = _fakers.Country.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Country>();
            dbContext.Countries.Add(country);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        var apiClient = new HeadersClient(httpClient);

        // Act
        ApiResponse<CountryPrimaryResponseDocument?> response1 = await ApiResponse.TranslateAsync(() => apiClient.GetCountryAsync(country.StringId!));
        string etag = response1.Headers[HeaderNames.ETag].First();

        ApiResponse<CountryPrimaryResponseDocument?> response2 =
            await ApiResponse.TranslateAsync(() => apiClient.GetCountryAsync(country.StringId!, if_None_Match: etag));

        // Assert
        response2.StatusCode.Should().Be((int)HttpStatusCode.NotModified);
        response2.Headers.Should().ContainKey(HeaderNames.ETag).WhoseValue.Should().Equal([etag]);
    }

    [Fact]
    public async Task Getting_twice_modified_resource_should_return_200()
    {
        // Arrange
        Country country = _fakers.Country.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Country>();
            dbContext.Countries.Add(country);
            await dbContext.SaveChangesAsync();
        });

        using HttpClient httpClient = _testContext.Factory.CreateClient();
        var apiClient = new HeadersClient(httpClient);

        // Act
        ApiResponse<CountryPrimaryResponseDocument?> response1 = await ApiResponse.TranslateAsync(() => apiClient.GetCountryAsync(country.StringId!));
        string etag = response1.Headers[HeaderNames.ETag].First();

        await ApiResponse.TranslateAsync(() => apiClient.PatchCountryAsync(country.StringId!, body: new CountryPatchRequestDocument
        {
            Data = new CountryDataInPatchRequest
            {
                Id = country.StringId!,
                Attributes = new CountryAttributesInPatchRequest
                {
                    Name = _fakers.Country.Generate().Name
                }
            }
        }));

        ApiResponse<CountryPrimaryResponseDocument?> response3 =
            await ApiResponse.TranslateAsync(() => apiClient.GetCountryAsync(country.StringId!, if_None_Match: etag));

        // Assert
        response3.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response3.Headers.Should().ContainKey(HeaderNames.ETag).WhoseValue.Should().NotEqual([etag]);
    }
}
