using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance
{
    public sealed class InheritanceTests : IClassFixture<IntegrationTestContext<TestableStartup<InheritanceDbContext>, InheritanceDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<InheritanceDbContext>, InheritanceDbContext> _testContext;

        public InheritanceTests(IntegrationTestContext<TestableStartup<InheritanceDbContext>, InheritanceDbContext> testContext)
        {
            _testContext = testContext;
        }
        
        [Fact]
        public async Task Can_create_resource_with_inherited_attributes()
        {
            // Arrange
            var man = new Man
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
                        familyName = man.FamilyName,
                        isRetired = man.IsRetired,
                        hasBeard = man.HasBeard
                    }
                }
            };

            var route = "/men";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Type.Should().Be("men");
            responseDocument.SingleData.Attributes["familyName"].Should().Be(man.FamilyName);
            responseDocument.SingleData.Attributes["isRetired"].Should().Be(man.IsRetired);
            responseDocument.SingleData.Attributes["hasBeard"].Should().Be(man.HasBeard);

            var newManId = int.Parse(responseDocument.SingleData.Id);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var manInDatabase = await dbContext.Men
                    .FirstAsync(m => m.Id == newManId);
                
                manInDatabase.FamilyName.Should().Be(man.FamilyName);
                manInDatabase.IsRetired.Should().Be(man.IsRetired);
                manInDatabase.HasBeard.Should().Be(man.HasBeard);
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

            var route = "/men";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            var newManId = int.Parse(responseDocument.SingleData.Id);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var manInDatabase = await dbContext.Men
                    .Include(man => man.HealthInsurance)
                    .FirstAsync(man => man.Id == newManId);
                
                manInDatabase.HealthInsurance.Should().BeOfType<CompanyHealthInsurance>();
                manInDatabase.HealthInsurance.Id.Should().Be(existingInsurance.Id);
            });
        }

        [Fact]
        public async Task Can_update_resource_through_primary_endpoint()
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

            var route = "/men/" + existingMan.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var manInDatabase = await dbContext.Men
                    .FirstAsync(man => man.Id == existingMan.Id);

                manInDatabase.FamilyName.Should().Be(newMan.FamilyName);
                manInDatabase.IsRetired.Should().Be(newMan.IsRetired);
                manInDatabase.HasBeard.Should().Be(newMan.HasBeard);
            });
        }

        [Fact]
        public async Task Can_update_resource_with_ToOne_relationship_through_relationship_endpoint()
        {
            // Arrange
            var existingMan = new Man();
            var existingInsurance = new CompanyHealthInsurance();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTablesAsync<Man, CompanyHealthInsurance>();
                dbContext.AddRange(existingMan, existingInsurance);

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

            var route = $"/men/{existingMan.StringId}/relationships/healthInsurance";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var manInDatabase = await dbContext.Men
                    .Include(man => man.HealthInsurance)
                    .FirstAsync(man => man.Id == existingMan.Id);

                manInDatabase.HealthInsurance.Should().BeOfType<CompanyHealthInsurance>();
                manInDatabase.HealthInsurance.Id.Should().Be(existingInsurance.Id);
            });
        }

        [Fact]
        public async Task Can_create_resource_with_ToMany_relationship()
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

            var route = "/men";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            var newManId = int.Parse(responseDocument.SingleData.Id);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var manInDatabase = await dbContext.Men
                    .Include(man => man.Parents)
                    .FirstAsync(man => man.Id == newManId);

                manInDatabase.Parents.Should().HaveCount(2);
                manInDatabase.Parents.Should().ContainSingle(human => human is Man);
                manInDatabase.Parents.Should().ContainSingle(human => human is Woman);
            });
        }

        [Fact]
        public async Task Can_update_resource_with_ToMany_relationship_through_relationship_endpoint()
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

            var route = $"/men/{existingChild.StringId}/relationships/parents";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);
        
            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var manInDatabase = await dbContext.Men
                    .Include(man => man.Parents)
                    .FirstAsync(man => man.Id == existingChild.Id);
                
                manInDatabase.Parents.Should().HaveCount(2);
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

            var route = "/men";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);
        
            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            var newManId = int.Parse(responseDocument.SingleData.Id);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var contentItems = await dbContext.HumanFavoriteContentItems
                    .Where(favorite => favorite.Human.Id == newManId)
                    .Select(favorite => favorite.ContentItem)
                    .ToListAsync();

                contentItems.Should().HaveCount(2);
                contentItems.Should().ContainSingle(item => item is Book);
                contentItems.Should().ContainSingle(item => item is Video);
            });
        }

        [Fact]
        public async Task Can_update_resource_with_ManyToMany_relationship_through_relationship_endpoint()
        {
            // Arrange
            var existingBook = new Book();
            var existingVideo = new Video();
            var existingMan = new Man();
            
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTablesAsync<HumanFavoriteContentItem, Book, Video, Man>();
                dbContext.AddRange(existingBook, existingVideo, existingMan);

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

            var route = $"/men/{existingMan.StringId}/relationships/favoriteContent";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);
            
            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var contentItems = await dbContext.HumanFavoriteContentItems
                    .Where(favorite => favorite.Human.Id == existingMan.Id)
                    .Select(favorite => favorite.ContentItem)
                    .ToListAsync();

                contentItems.Should().HaveCount(2);
                contentItems.Should().ContainSingle(item => item is Book);
                contentItems.Should().ContainSingle(item => item is Video);
            });
        }
    }
}
