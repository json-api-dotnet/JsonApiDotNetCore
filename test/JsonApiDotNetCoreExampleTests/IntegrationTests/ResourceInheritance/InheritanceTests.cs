using System.Collections.Generic;
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
        public async Task Can_create_resource_with_to_one_relationship()
        {
            // Arrange
            var insurance = new CompanyHealthInsurance();
            
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<CompanyHealthInsurance>();
                dbContext.CompanyHealthInsurances.Add(insurance);
                await dbContext.SaveChangesAsync();
            });

            var route = "/men";
            var requestBody = new
            {
                data = new
                {
                    type = "men",
                    relationships = new Dictionary<string, object>
                    {
                        {
                            "healthInsurance", new
                            {
                                data = new { type = "companyHealthInsurances", id = insurance.StringId }
                            }
                        }
                    }
                }
            };

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var manInDatabase = await dbContext.Men
                    .Include(m => m.HealthInsurance)
                    .SingleAsync(m => m.Id == int.Parse(responseDocument.SingleData.Id));
                
                manInDatabase.HealthInsurance.Should().BeOfType<CompanyHealthInsurance>();
            });
        }
        
        [Fact]
        public async Task Can_patch_resource_with_to_one_relationship_through_relationship_link()
        {
            // Arrange
            var man = new Man();
            var insurance = new CompanyHealthInsurance();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTablesAsync<Man, CompanyHealthInsurance>();
                dbContext.AddRange(man, insurance);
                await dbContext.SaveChangesAsync();
            });
            
            var route = $"/men/{man.Id}/relationships/healthInsurance";

            var requestBody = new
            {
                data = new { type = "companyHealthInsurances", id = insurance.StringId }
            };

            // Act
            var (httpResponse, _) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var manInDatabase = await dbContext.Men
                    .Include(m => m.HealthInsurance)
                    .SingleAsync(h => h.Id == man.Id);

                manInDatabase.HealthInsurance.Should().BeOfType<CompanyHealthInsurance>();
            });
        }


        [Fact]
        public async Task Can_create_resource_with_to_many_relationship()
        {
            // Arrange
            var father = new Man();
            var mother = new Woman();
            
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTablesAsync<Woman, Man>();
                dbContext.Humans.AddRange(father, mother);
                await dbContext.SaveChangesAsync();
            });

            var route = "/men";
            var requestBody = new
            {
                data = new
                {
                    type = "men",
                    relationships = new Dictionary<string, object>
                    {
                        {
                            "parents", new
                            {
                                data = new[]
                                {
                                    new { type = "men", id = father.StringId },
                                    new { type = "women", id = mother.StringId }
                                }
                            }
                        }
                    }
                }
            };

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var manInDatabase = await dbContext.Men
                    .Include(m => m.Parents)
                    .SingleAsync(m => m.Id == int.Parse(responseDocument.SingleData.Id));

                manInDatabase.Parents.Should().HaveCount(2);
                manInDatabase.Parents.Should().ContainSingle(h => h is Man);
                manInDatabase.Parents.Should().ContainSingle(h => h is Woman);
            });
        }

        [Fact]
        public async Task Can_patch_resource_with_to_many_relationship_through_relationship_link()
        {
            // Arrange
            var child = new Man();
            var father = new Man();
            var mother = new Woman();
            
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTablesAsync<Woman, Man>();
                dbContext.Humans.AddRange(child, father, mother);
                await dbContext.SaveChangesAsync();
            });
        
            var route = $"/men/{child.StringId}/relationships/parents";
            var requestBody = new
            {
                data = new[]
                {
                    new { type = "men", id = father.StringId },
                    new { type = "women", id = mother.StringId }
                }
            };
        
            // Act
            var (httpResponse, _) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);
        
            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var manInDatabase = await dbContext.Men
                    .Include(m => m.Parents)
                    .SingleAsync(m => m.Id == child.Id);
                
                manInDatabase.Parents.Should().HaveCount(2);
                manInDatabase.Parents.Should().ContainSingle(h => h is Man);
                manInDatabase.Parents.Should().ContainSingle(h => h is Woman);
            });
        }

        [Fact]
        public async Task Can_create_resource_with_many_to_many_relationship()
        {
            // Arrange
            var book = new Book();
            var video = new Video();
            
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTablesAsync<Book, Video, Man>();
                dbContext.ContentItems.AddRange(book, video);
                await dbContext.SaveChangesAsync();
            });
        
            var route = "/men";
            var requestBody = new
            {
                data = new
                {
                    type = "men",
                    relationships = new Dictionary<string, object>
                    {
                        {
                            "favoriteContent", new
                            {
                                data = new[]
                                {
                                    new { type = "books", id = book.StringId },
                                    new { type = "videos", id = video.StringId }
                                }
                            }
                        }
                    }
                }
            };
        
            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);
        
            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var contentItems = await dbContext.HumanFavoriteContentItems
                    .Where(hfci => hfci.Human.Id == int.Parse(responseDocument.SingleData.Id))
                    .Select(hfci => hfci.ContentItem)
                    .ToListAsync();
                
                contentItems.Should().HaveCount(2);
                contentItems.Should().ContainSingle(ci => ci is Book);
                contentItems.Should().ContainSingle(ci => ci is Video);
            });
        }

        [Fact]
        public async Task Can_patch_resource_with_many_to_many_relationship_through_relationship_link()
        {
            // Arrange
            var book = new Book();
            var video = new Video();
            var man = new Man();
            
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTablesAsync<HumanFavoriteContentItem, Book, Video, Man>();
                dbContext.AddRange(book, video, man);
                await dbContext.SaveChangesAsync();
            });
        
            var route = $"/men/{man.Id}/relationships/favoriteContent";
            var requestBody = new
            {
                data = new[]
                {
                    new { type = "books", id = book.StringId },
                    new { type = "videos", id = video.StringId }
                }
            };
        
            // Act
            var (httpResponse, _) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);
        
            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var contentItems = await dbContext.HumanFavoriteContentItems
                    .Where(hfci => hfci.Human.Id == man.Id)
                    .Select(hfci => hfci.ContentItem)
                    .ToListAsync();

                contentItems.Should().HaveCount(2);
                contentItems.Should().ContainSingle(ci => ci is Book);
                contentItems.Should().ContainSingle(ci => ci is Video);
            });
        }
    }
}
