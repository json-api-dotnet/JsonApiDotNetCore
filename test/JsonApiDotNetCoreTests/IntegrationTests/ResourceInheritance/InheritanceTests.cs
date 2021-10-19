#nullable disable

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Models;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance
{
    public sealed class InheritanceTests : IClassFixture<IntegrationTestContext<TestableStartup<InheritanceDbContext>, InheritanceDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<InheritanceDbContext>, InheritanceDbContext> _testContext;

        public InheritanceTests(IntegrationTestContext<TestableStartup<InheritanceDbContext>, InheritanceDbContext> testContext)
        {
            _testContext = testContext;

            testContext.UseController<MenController>();
        }

        [Fact]
        public async Task Can_create_resource_with_inherited_attributes()
        {
            // Arrange
            var newMan = new Man
            {
                FamilyName = "Smith",
                IsRetired = true,
                HasBeard = true
            };

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
            responseDocument.Data.SingleValue.Attributes["familyName"].Should().Be(newMan.FamilyName);
            responseDocument.Data.SingleValue.Attributes["isRetired"].Should().Be(newMan.IsRetired);
            responseDocument.Data.SingleValue.Attributes["hasBeard"].Should().Be(newMan.HasBeard);

            int newManId = int.Parse(responseDocument.Data.SingleValue.Id);

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
            var existingInsurance = new CompanyHealthInsurance();

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
            int newManId = int.Parse(responseDocument.Data.SingleValue.Id);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                Man manInDatabase = await dbContext.Men.Include(man => man.HealthInsurance).FirstWithIdAsync(newManId);

                manInDatabase.HealthInsurance.Should().BeOfType<CompanyHealthInsurance>();
                manInDatabase.HealthInsurance.Id.Should().Be(existingInsurance.Id);
            });
        }

        [Fact]
        public async Task Can_update_resource()
        {
            // Arrange
            var existingMan = new Man
            {
                FamilyName = "Smith",
                IsRetired = false,
                HasBeard = true
            };

            var newMan = new Man
            {
                FamilyName = "Jackson",
                IsRetired = true,
                HasBeard = false
            };

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
            var existingMan = new Man();
            var existingInsurance = new CompanyHealthInsurance();

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
                    type = "companyHealthInsurances",
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

                manInDatabase.HealthInsurance.Should().BeOfType<CompanyHealthInsurance>();
                manInDatabase.HealthInsurance.Id.Should().Be(existingInsurance.Id);
            });
        }

        [Fact]
        public async Task Can_create_resource_with_OneToMany_relationship()
        {
            // Arrange
            var existingFather = new Man();
            var existingMother = new Woman();

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
            int newManId = int.Parse(responseDocument.Data.SingleValue.Id);

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
            var existingChild = new Man();
            var existingFather = new Man();
            var existingMother = new Woman();

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
            var existingBook = new Book();
            var existingVideo = new Video();

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
            int newManId = int.Parse(responseDocument.Data.SingleValue.Id);

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
            var existingBook = new Book();
            var existingVideo = new Video();
            var existingMan = new Man();

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
}
