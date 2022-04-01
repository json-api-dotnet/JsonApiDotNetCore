using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.Blobs;

public sealed class BlobTests : IClassFixture<IntegrationTestContext<TestableStartup<BlobDbContext>, BlobDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<BlobDbContext>, BlobDbContext> _testContext;
    private readonly BlobFakers _fakers = new();

    public BlobTests(IntegrationTestContext<TestableStartup<BlobDbContext>, BlobDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<ImageContainersController>();

        testContext.ConfigureServicesAfterStartup(services =>
        {
            services.AddScoped(typeof(IResourceChangeTracker<>), typeof(NeverSameResourceChangeTracker<>));
        });
    }

    [Fact]
    public async Task Can_get_primary_resource_by_ID()
    {
        // Arrange
        ImageContainer container = _fakers.ImageContainer.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.ImageContainers.Add(container);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/imageContainers/{container.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("imageContainers");
        responseDocument.Data.SingleValue.Id.Should().Be(container.StringId);
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("fileName").With(value => value.Should().Be(container.FileName));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("data").As<byte[]>().With(value => value.Should().Equal(container.Data));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("thumbnail").As<byte[]>().With(value => value.Should().Equal(container.Thumbnail));
        responseDocument.Data.SingleValue.Relationships.Should().BeNull();
    }

    [Fact]
    public async Task Can_create_resource()
    {
        // Arrange
        ImageContainer newContainer = _fakers.ImageContainer.Generate();

        var requestBody = new
        {
            data = new
            {
                type = "imageContainers",
                attributes = new
                {
                    fileName = newContainer.FileName,
                    data = Convert.ToBase64String(newContainer.Data),
                    thumbnail = Convert.ToBase64String(newContainer.Thumbnail!)
                }
            }
        };

        const string route = "/imageContainers";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("imageContainers");
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("fileName").With(value => value.Should().Be(newContainer.FileName));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("data").As<byte[]>().With(value => value.Should().Equal(newContainer.Data));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("thumbnail").As<byte[]>().With(value => value.Should().Equal(newContainer.Thumbnail));
        responseDocument.Data.SingleValue.Relationships.Should().BeNull();

        long newContainerId = long.Parse(responseDocument.Data.SingleValue.Id.ShouldNotBeNull());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            ImageContainer containerInDatabase = await dbContext.ImageContainers.FirstWithIdAsync(newContainerId);

            containerInDatabase.FileName.Should().Be(newContainer.FileName);
            containerInDatabase.Data.Should().Equal(newContainer.Data);
            containerInDatabase.Thumbnail.Should().Equal(newContainer.Thumbnail);
        });
    }

    [Fact]
    public async Task Can_update_resource()
    {
        // Arrange
        ImageContainer existingContainer = _fakers.ImageContainer.Generate();

        byte[] newData = _fakers.ImageContainer.Generate().Data;
        byte[] newThumbnail = _fakers.ImageContainer.Generate().Thumbnail!;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.ImageContainers.Add(existingContainer);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "imageContainers",
                id = existingContainer.StringId,
                attributes = new
                {
                    data = Convert.ToBase64String(newData),
                    thumbnail = Convert.ToBase64String(newThumbnail)
                }
            }
        };

        string route = $"/imageContainers/{existingContainer.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("imageContainers");
        responseDocument.Data.SingleValue.Id.Should().Be(existingContainer.StringId);
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("fileName").With(value => value.Should().Be(existingContainer.FileName));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("data").As<byte[]>().With(value => value.Should().Equal(newData));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("thumbnail").As<byte[]>().With(value => value.Should().Equal(newThumbnail));
        responseDocument.Data.SingleValue.Relationships.Should().BeNull();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            ImageContainer containerInDatabase = await dbContext.ImageContainers.FirstWithIdAsync(existingContainer.Id);

            containerInDatabase.FileName.Should().Be(existingContainer.FileName);
            containerInDatabase.Data.Should().Equal(newData);
            containerInDatabase.Thumbnail.Should().Equal(newThumbnail);
        });
    }

    [Fact]
    public async Task Can_update_resource_with_empty_blob()
    {
        // Arrange
        ImageContainer existingContainer = _fakers.ImageContainer.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.ImageContainers.Add(existingContainer);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "imageContainers",
                id = existingContainer.StringId,
                attributes = new
                {
                    data = string.Empty
                }
            }
        };

        string route = $"/imageContainers/{existingContainer.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("imageContainers");
        responseDocument.Data.SingleValue.Id.Should().Be(existingContainer.StringId);
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("fileName").With(value => value.Should().Be(existingContainer.FileName));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("data").As<byte[]>().With(value => value.Should().BeEmpty());
        responseDocument.Data.SingleValue.Relationships.Should().BeNull();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            ImageContainer containerInDatabase = await dbContext.ImageContainers.FirstWithIdAsync(existingContainer.Id);

            containerInDatabase.FileName.Should().Be(existingContainer.FileName);
            containerInDatabase.Data.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task Can_update_resource_with_null_blob()
    {
        // Arrange
        ImageContainer existingContainer = _fakers.ImageContainer.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.ImageContainers.Add(existingContainer);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "imageContainers",
                id = existingContainer.StringId,
                attributes = new
                {
                    thumbnail = (object?)null
                }
            }
        };

        string route = $"/imageContainers/{existingContainer.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("imageContainers");
        responseDocument.Data.SingleValue.Id.Should().Be(existingContainer.StringId);
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("fileName").With(value => value.Should().Be(existingContainer.FileName));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("thumbnail").With(value => value.Should().BeNull());
        responseDocument.Data.SingleValue.Relationships.Should().BeNull();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            ImageContainer containerInDatabase = await dbContext.ImageContainers.FirstWithIdAsync(existingContainer.Id);

            containerInDatabase.FileName.Should().Be(existingContainer.FileName);
            containerInDatabase.Thumbnail.Should().BeNull();
        });
    }
}
