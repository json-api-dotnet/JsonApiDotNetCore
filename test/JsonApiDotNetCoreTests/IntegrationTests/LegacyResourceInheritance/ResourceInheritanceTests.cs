using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreTests.IntegrationTests.LegacyResourceInheritance.Models;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.LegacyResourceInheritance;

public sealed class ResourceInheritanceTests
    : IClassFixture<IntegrationTestContext<TestableStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> _testContext;

    public ResourceInheritanceTests(IntegrationTestContext<TestableStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<AnimalsController>();
        testContext.UseController<CatsController>();
        testContext.UseController<DogsController>();

        testContext.UseController<PeopleController>();
        testContext.UseController<MalesController>();
        testContext.UseController<FemalesController>();

        testContext.UseController<ContentsController>();
        testContext.UseController<BooksController>();
        testContext.UseController<VideosController>();

        _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTablesAsync<Animal, Content, Person>();
        }).Wait();
    }

    [Fact]
    public async Task Can_create_resource_with_to_one_relationship()
    {
        // Arrange
        var newCat = new Cat();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.AddAsync(newCat);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "males",
                relationships = new Dictionary<string, object>
                {
                    {
                        "pet", new
                        {
                            data = new
                            {
                                type = "cats",
                                id = newCat.StringId
                            }
                        }
                    }
                }
            }
        };

        const string route = "/people";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.ShouldNotBeNull();

        long newPersonId = long.Parse(responseDocument.Data.SingleValue.Id!);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Person personInDatabase = await dbContext.People.Include(person => person.Pet).FirstWithIdAsync(newPersonId);

            personInDatabase.Pet.Should().NotBeNull();
            personInDatabase.Pet!.GetType().Should().Be(newCat.GetType());
        });
    }

    [Fact]
    public async Task Can_patch_resource_with_to_one_relationship_through_relationship_link()
    {
        // Arrange
        var existingPerson = new Male();
        var existingCat = new Cat();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.AddRangeAsync(existingPerson, existingCat);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "cats",
                id = existingCat.StringId
            }
        };

        string route = $"/people/{existingPerson.Id}/relationships/pet";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Person personInDatabase = await dbContext.People.Include(person => person.Pet).FirstWithIdAsync(existingPerson.Id);

            personInDatabase.Pet.ShouldNotBeNull();
            personInDatabase.Pet.GetType().Should().Be(existingCat.GetType());
        });
    }

    [Fact]
    public async Task Can_create_resource_with_to_many_relationship()
    {
        // Arrange
        var existingFather = new Male();
        var existingMother = new Female();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.AddRangeAsync(existingFather, existingMother);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "males",
                relationships = new Dictionary<string, object>
                {
                    {
                        "parents", new
                        {
                            data = new[]
                            {
                                new
                                {
                                    type = "males",
                                    id = existingFather.StringId
                                },
                                new
                                {
                                    type = "females",
                                    id = existingMother.StringId
                                }
                            }
                        }
                    }
                }
            }
        };

        const string route = "/people";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.ShouldNotBeNull();

        long newPersonId = long.Parse(responseDocument.Data.SingleValue.Id!);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Person personInDatabase = await dbContext.People.Include(person => person.Parents).FirstWithIdAsync(newPersonId);

            personInDatabase.Parents.Should().HaveCount(2);
            personInDatabase.Parents.Should().ContainSingle(person => person is Male);
            personInDatabase.Parents.Should().ContainSingle(person => person is Female);
        });
    }

    [Fact]
    public async Task Can_patch_resource_with_to_many_relationship_through_relationship_link()
    {
        // Arrange
        var existingChild = new Male();
        var existingFather = new Male();
        var existingMother = new Female();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.AddRangeAsync(existingChild, existingFather, existingMother);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "males",
                    id = existingFather.StringId
                },
                new
                {
                    type = "females",
                    id = existingMother.StringId
                }
            }
        };

        string route = $"/people/{existingChild.StringId}/relationships/parents";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Male childInDatabase = await dbContext.Males.Include(male => male.Parents).FirstWithIdAsync(existingChild.Id);

            childInDatabase.Parents.Should().HaveCount(2);
            childInDatabase.Parents.Should().ContainSingle(person => person is Male);
            childInDatabase.Parents.Should().ContainSingle(person => person is Female);
        });
    }

    [Fact]
    public async Task Can_create_resource_with_many_to_many_relationship()
    {
        // Arrange
        var existingBook = new Book();
        var existingVideo = new Video();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.AddRangeAsync(existingBook, existingVideo);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "males",
                relationships = new Dictionary<string, object>
                {
                    {
                        "favoriteContent", new
                        {
                            data = new[]
                            {
                                new
                                {
                                    type = "books",
                                    id = existingBook.StringId
                                },
                                new
                                {
                                    type = "videos",
                                    id = existingVideo.StringId
                                }
                            }
                        }
                    }
                }
            }
        };

        const string route = "/males?include=favoriteContent";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.ShouldNotBeNull();

        long newPersonId = long.Parse(responseDocument.Data.SingleValue.Id!);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Person personInDatabase = await dbContext.People.Include(person => person.FavoriteContent).FirstWithIdAsync(newPersonId);

            personInDatabase.FavoriteContent.Should().HaveCount(2);
            personInDatabase.FavoriteContent.Should().ContainSingle(content => content is Book);
            personInDatabase.FavoriteContent.Should().ContainSingle(content => content is Video);
        });
    }

    [Fact]
    public async Task Can_patch_resource_with_many_to_many_relationship_through_relationship_link()
    {
        // Arrange
        var existingNook = new Book();
        var existingVideo = new Video();
        var existingPerson = new Male();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.AddRangeAsync(existingNook, existingVideo, existingPerson);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "books",
                    id = existingNook.StringId
                },
                new
                {
                    type = "videos",
                    id = existingVideo.StringId
                }
            }
        };

        string route = $"/people/{existingPerson.Id}/relationships/favoriteContent";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Male maleInDatabase = await dbContext.Males.Include(male => male.FavoriteContent).FirstWithIdAsync(existingPerson.Id);

            maleInDatabase.FavoriteContent.Should().HaveCount(2);
            maleInDatabase.FavoriteContent.Should().ContainSingle(content => content is Book);
            maleInDatabase.FavoriteContent.Should().ContainSingle(content => content is Video);
        });
    }
}
