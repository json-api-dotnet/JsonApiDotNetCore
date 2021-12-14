using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance;

public sealed class InheritanceTests : IClassFixture<IntegrationTestContext<TestableStartup<InheritanceDbContext>, InheritanceDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<InheritanceDbContext>, InheritanceDbContext> _testContext;
    private readonly InheritanceFakers _fakers = new();

    public InheritanceTests(IntegrationTestContext<TestableStartup<InheritanceDbContext>, InheritanceDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<MenController>();
    }

    [Fact]
    public async Task Can_create_resource_with_inherited_attributes()
    {
        // Arrange
        Man newMan = _fakers.Man.Generate();

        var requestBody = new
        {
            data = new
            {
                type = "men",
                attributes = new
                {
                    familyName = newMan.FamilyName,
                    isRetired = newMan.IsRetired,
                    hasBeard = newMan.HasBeard
                }
            }
        };

        const string route = "/men";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("men");
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("familyName").With(value => value.Should().Be(newMan.FamilyName));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("isRetired").With(value => value.Should().Be(newMan.IsRetired));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("hasBeard").With(value => value.Should().Be(newMan.HasBeard));

        int newManId = int.Parse(responseDocument.Data.SingleValue.Id.ShouldNotBeNull());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Man manInDatabase = await dbContext.Men.FirstWithIdAsync(newManId);

            manInDatabase.FamilyName.Should().Be(newMan.FamilyName);
            manInDatabase.IsRetired.Should().Be(newMan.IsRetired);
            manInDatabase.HasBeard.Should().Be(newMan.HasBeard);
        });
    }

    [Fact]
    public async Task Can_create_resource_with_ToOne_relationship()
    {
        // Arrange
        CompanyHealthInsurance existingInsurance = _fakers.CompanyHealthInsurance.Generate();

        string newFamilyName = _fakers.Man.Generate().FamilyName;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<CompanyHealthInsurance>();
            dbContext.CompanyHealthInsurances.Add(existingInsurance);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "men",
                attributes = new
                {
                    familyName = newFamilyName
                },
                relationships = new
                {
                    healthInsurance = new
                    {
                        data = new
                        {
                            type = "companyHealthInsurances",
                            id = existingInsurance.StringId
                        }
                    }
                }
            }
        };

        const string route = "/men";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        int newManId = int.Parse(responseDocument.Data.SingleValue.Id.ShouldNotBeNull());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Man manInDatabase = await dbContext.Men.Include(man => man.HealthInsurance).FirstWithIdAsync(newManId);

            manInDatabase.HealthInsurance.ShouldNotBeNull();
            manInDatabase.HealthInsurance.Should().BeOfType<CompanyHealthInsurance>();
            manInDatabase.HealthInsurance.Id.Should().Be(existingInsurance.Id);
        });
    }

    [Fact]
    public async Task Can_update_resource()
    {
        // Arrange
        Man existingMan = _fakers.Man.Generate();

        Man newMan = _fakers.Man.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Men.Add(existingMan);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "men",
                id = existingMan.StringId,
                attributes = new
                {
                    familyName = newMan.FamilyName,
                    isRetired = newMan.IsRetired,
                    hasBeard = newMan.HasBeard
                }
            }
        };

        string route = $"/men/{existingMan.StringId}";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Man manInDatabase = await dbContext.Men.FirstWithIdAsync(existingMan.Id);

            manInDatabase.FamilyName.Should().Be(newMan.FamilyName);
            manInDatabase.IsRetired.Should().Be(newMan.IsRetired);
            manInDatabase.HasBeard.Should().Be(newMan.HasBeard);
        });
    }

    [Fact]
    public async Task Can_assign_ToOne_relationship()
    {
        // Arrange
        Man existingMan = _fakers.Man.Generate();
        FamilyHealthInsurance existingInsurance = _fakers.FamilyHealthInsurance.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTablesAsync<Man, CompanyHealthInsurance>();
            dbContext.AddInRange(existingMan, existingInsurance);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "familyHealthInsurances",
                id = existingInsurance.StringId
            }
        };

        string route = $"/men/{existingMan.StringId}/relationships/healthInsurance";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Man manInDatabase = await dbContext.Men.Include(man => man.HealthInsurance).FirstWithIdAsync(existingMan.Id);

            manInDatabase.HealthInsurance.ShouldNotBeNull();
            manInDatabase.HealthInsurance.Should().BeOfType<FamilyHealthInsurance>();
            manInDatabase.HealthInsurance.Id.Should().Be(existingInsurance.Id);
        });
    }

    [Fact]
    public async Task Can_create_resource_with_OneToMany_relationship()
    {
        // Arrange
        Man existingFather = _fakers.Man.Generate();
        Woman existingMother = _fakers.Woman.Generate();

        string newFamilyName = _fakers.Man.Generate().FamilyName;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTablesAsync<Woman, Man>();
            dbContext.Humans.AddRange(existingFather, existingMother);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "men",
                attributes = new
                {
                    familyName = newFamilyName
                },
                relationships = new
                {
                    parents = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "men",
                                id = existingFather.StringId
                            },
                            new
                            {
                                type = "women",
                                id = existingMother.StringId
                            }
                        }
                    }
                }
            }
        };

        const string route = "/men";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        int newManId = int.Parse(responseDocument.Data.SingleValue.Id.ShouldNotBeNull());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Man manInDatabase = await dbContext.Men.Include(man => man.Parents).FirstWithIdAsync(newManId);

            manInDatabase.Parents.ShouldHaveCount(2);
            manInDatabase.Parents.Should().ContainSingle(human => human is Man);
            manInDatabase.Parents.Should().ContainSingle(human => human is Woman);
        });
    }

    [Fact]
    public async Task Can_assign_OneToMany_relationship()
    {
        // Arrange
        Man existingChild = _fakers.Man.Generate();
        Man existingFather = _fakers.Man.Generate();
        Woman existingMother = _fakers.Woman.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTablesAsync<Woman, Man>();
            dbContext.Humans.AddRange(existingChild, existingFather, existingMother);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "men",
                    id = existingFather.StringId
                },
                new
                {
                    type = "women",
                    id = existingMother.StringId
                }
            }
        };

        string route = $"/men/{existingChild.StringId}/relationships/parents";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Man manInDatabase = await dbContext.Men.Include(man => man.Parents).FirstWithIdAsync(existingChild.Id);

            manInDatabase.Parents.ShouldHaveCount(2);
            manInDatabase.Parents.Should().ContainSingle(human => human is Man);
            manInDatabase.Parents.Should().ContainSingle(human => human is Woman);
        });
    }

    [Fact]
    public async Task Can_create_resource_with_ManyToMany_relationship()
    {
        // Arrange
        Book existingBook = _fakers.Book.Generate();
        Video existingVideo = _fakers.Video.Generate();

        string newFamilyName = _fakers.Man.Generate().FamilyName;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTablesAsync<Book, Video, Man>();
            dbContext.ContentItems.AddRange(existingBook, existingVideo);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "men",
                attributes = new
                {
                    familyName = newFamilyName
                },
                relationships = new
                {
                    favoriteContent = new
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
        };

        const string route = "/men";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        int newManId = int.Parse(responseDocument.Data.SingleValue.Id.ShouldNotBeNull());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Man manInDatabase = await dbContext.Men.Include(man => man.FavoriteContent).FirstWithIdAsync(newManId);

            manInDatabase.FavoriteContent.ShouldHaveCount(2);
            manInDatabase.FavoriteContent.Should().ContainSingle(item => item is Book && item.Id == existingBook.Id);
            manInDatabase.FavoriteContent.Should().ContainSingle(item => item is Video && item.Id == existingVideo.Id);
        });
    }

    [Fact]
    public async Task Can_assign_ManyToMany_relationship()
    {
        // Arrange
        Book existingBook = _fakers.Book.Generate();
        Video existingVideo = _fakers.Video.Generate();
        Man existingMan = _fakers.Man.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTablesAsync<Book, Video, Man>();
            dbContext.AddInRange(existingBook, existingVideo, existingMan);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
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
        };

        string route = $"/men/{existingMan.StringId}/relationships/favoriteContent";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            Man manInDatabase = await dbContext.Men.Include(man => man.FavoriteContent).FirstWithIdAsync(existingMan.Id);

            manInDatabase.FavoriteContent.ShouldHaveCount(2);
            manInDatabase.FavoriteContent.Should().ContainSingle(item => item is Book && item.Id == existingBook.Id);
            manInDatabase.FavoriteContent.Should().ContainSingle(item => item is Video && item.Id == existingVideo.Id);
        });
    }
}
