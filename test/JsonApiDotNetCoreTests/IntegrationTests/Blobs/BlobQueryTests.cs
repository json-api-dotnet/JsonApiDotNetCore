using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.Blobs;

// A byte[] attribute is a collection of bytes. These tests cover the query string functions that apply to collections.
public sealed class BlobQueryTests : IClassFixture<IntegrationTestContext<TestableStartup<BlobDbContext>, BlobDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<BlobDbContext>, BlobDbContext> _testContext;
    private readonly BlobFakers _fakers = new();

    public BlobQueryTests(IntegrationTestContext<TestableStartup<BlobDbContext>, BlobDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<ImageContainersController>();
    }

    [Fact]
    public async Task Can_filter_on_count_of_bytes()
    {
        // Arrange
        (ImageContainer pngLike, ImageContainer small, ImageContainer empty) = await CreateContainersAsync();

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) =
            await _testContext.ExecuteGetAsync<Document>("/imageContainers?filter=greaterThan(count(data),'3')");

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(pngLike.StringId);
        responseDocument.Data.ManyValue.Should().NotContain(resource => resource.Id == small.StringId || resource.Id == empty.StringId);
    }

    [Fact]
    public async Task Can_sort_on_count_of_bytes()
    {
        // Arrange
        (ImageContainer pngLike, ImageContainer small, ImageContainer empty) = await CreateContainersAsync();

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>("/imageContainers?sort=-count(data)");

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(3);
        responseDocument.Data.ManyValue[0].Id.Should().Be(pngLike.StringId);
        responseDocument.Data.ManyValue[1].Id.Should().Be(small.StringId);
        responseDocument.Data.ManyValue[2].Id.Should().Be(empty.StringId);
    }

    [Fact]
    public async Task Can_filter_on_has_bytes()
    {
        // Arrange
        (ImageContainer pngLike, ImageContainer small, ImageContainer empty) = await CreateContainersAsync();

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>("/imageContainers?filter=has(data)");

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);
        responseDocument.Data.ManyValue.Should().Contain(resource => resource.Id == pngLike.StringId);
        responseDocument.Data.ManyValue.Should().Contain(resource => resource.Id == small.StringId);
        responseDocument.Data.ManyValue.Should().NotContain(resource => resource.Id == empty.StringId);
    }

    [Fact]
    public async Task Can_filter_on_byte_value_in_blob()
    {
        // Arrange
        (ImageContainer pngLike, _, _) = await CreateContainersAsync();

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) =
            await _testContext.ExecuteGetAsync<Document>("/imageContainers?filter=has(data,equals(_,'137'))");

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(pngLike.StringId);
    }

    [Fact(Skip =
        "Npgsql 10 cannot translate Any(predicate) over bytea for comparisons other than equality: 'The LINQ expression b => b > @value could not be translated'.")]
    public async Task Can_filter_on_byte_range_in_blob()
    {
        // Arrange
        (ImageContainer pngLike, _, _) = await CreateContainersAsync();

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) =
            await _testContext.ExecuteGetAsync<Document>("/imageContainers?filter=has(data,greaterThan(_,'130'))");

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(pngLike.StringId);
    }

    private async Task<(ImageContainer PngLike, ImageContainer Small, ImageContainer Empty)> CreateContainersAsync()
    {
        ImageContainer pngLike = _fakers.ImageContainer.GenerateOne();

        pngLike.Data =
        [
            137,
            80,
            78,
            71,
            13,
            10
        ];

        ImageContainer small = _fakers.ImageContainer.GenerateOne();

        small.Data =
        [
            1,
            2,
            3
        ];

        ImageContainer empty = _fakers.ImageContainer.GenerateOne();
        empty.Data = [];

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<ImageContainer>();
            dbContext.ImageContainers.AddRange(pngLike, small, empty);
            await dbContext.SaveChangesAsync();
        });

        return (pngLike, small, empty);
    }
}
