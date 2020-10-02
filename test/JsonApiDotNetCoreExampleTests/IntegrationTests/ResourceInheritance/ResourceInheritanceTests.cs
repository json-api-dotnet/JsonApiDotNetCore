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
    public sealed class ResourceInheritanceTests : IClassFixture<IntegrationTestContext<TestableStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> _testContext;

        public ResourceInheritanceTests(IntegrationTestContext<TestableStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> testContext)
        {
            _testContext = testContext;

            _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<HumanContentItem>();
                await dbContext.ClearTableAsync<Book>();
                await dbContext.ClearTableAsync<Video>();
                await dbContext.ClearTableAsync<Cat>();
                await dbContext.ClearTableAsync<Dog>();
                await dbContext.ClearTableAsync<Female>();
                await dbContext.ClearTableAsync<Male>();

                await dbContext.SaveChangesAsync();
            }).Wait();
        }
        
        [Fact]
        public async Task Can_create_resource_with_to_one_relationship()
        {
            // Arrange
            var cat = new Cat();
            
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddAsync(cat);
                await dbContext.SaveChangesAsync();
            });

            var route = "/males";
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
                                data = new { type = "cats", id = cat.StringId }
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
                var assertPerson = await dbContext.Males
                    .Include(h => h.Pet)
                    .SingleAsync(h => h.Id == int.Parse(responseDocument.SingleData.Id));
                
                assertPerson.Pet.GetType().Should().Be(cat.GetType());
            });
        }


        [Fact]
        public async Task Can_patch_resource_with_to_one_relationship_through_relationship_link()
        {
            // Arrange
            var person = new Male();
            var cat = new Cat();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.AddRangeAsync(person, cat);
                await dbContext.SaveChangesAsync();
            });
            
            var route = $"/males/{person.Id}/relationships/pet";

            var requestBody = new
            {
                data = new { type = "cats", id = cat.StringId }
            };

            // Act
            var (httpResponse, _) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var assertPerson = await dbContext.Males
                    .Include(h => h.Pet)
                    .SingleAsync(h => h.Id.Equals(person.Id));

                assertPerson.Pet.GetType().Should().Be(cat.GetType());
            });
        }


        [Fact]
        public async Task Can_create_resource_with_to_many_relationship()
        {
            // Arrange
            var father = new Male();
            var mother = new Female();
            
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.AddRangeAsync(father, mother);
                await dbContext.SaveChangesAsync();
            });

            var route = "/males";
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
                                    new { type = "males", id = father.StringId },
                                    new { type = "females", id = mother.StringId }
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
                var assertPerson = await dbContext.Males
                    .Include(h => h.Parents)
                    .SingleAsync(h => h.Id == int.Parse(responseDocument.SingleData.Id));

                assertPerson.Parents.Should().HaveCount(2);
                assertPerson.Parents.Should().ContainSingle(h => h is Male);
                assertPerson.Parents.Should().ContainSingle(h => h is Female);
            });
        }

        [Fact]
        public async Task Can_patch_resource_with_to_many_relationship_through_relationship_link()
        {
            // Arrange   
            var child = new Male();
            var father = new Male();
            var mother = new Female();
            
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.AddRangeAsync(child, father, mother);
                await dbContext.SaveChangesAsync();
            });
        
            var route = $"/males/{child.StringId}/relationships/parents";
            var requestBody = new
            {
                data = new[]
                {
                    new { type = "males", id = father.StringId },
                    new { type = "females", id = mother.StringId }
                }
            };
        
            // Act
            var (httpResponse, _) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);
        
            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var assertChild = await dbContext.Males
                    .Include(h => h.Parents)
                    .SingleAsync(h => h.Id == child.Id);
                
                assertChild.Parents.Should().HaveCount(2);
                assertChild.Parents.Should().ContainSingle(h => h is Male);
                assertChild.Parents.Should().ContainSingle(h => h is Female);
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
                await dbContext.AddRangeAsync(book, video);
                await dbContext.SaveChangesAsync();
            });
        
            var route = "/males";
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
                var favoriteContent = (await dbContext.Males
                        .Include(h => h.HumanContentItems)
                            .ThenInclude(hp => hp.Content)
                        .SingleAsync(h => h.Id == int.Parse(responseDocument.SingleData.Id)))
                    .HumanContentItems.Select(hp => hp.Content).ToList();
                
                favoriteContent.Should().HaveCount(2);
                favoriteContent.Should().ContainSingle(h => h is Book);
                favoriteContent.Should().ContainSingle(h => h is Video);
            });
        }

        [Fact]
        public async Task Can_patch_resource_with_many_to_many_relationship_through_relationship_link()
        {
            // Arrange
            var book = new Book();
            var video = new Video();
            var person = new Male();
            
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.AddRangeAsync(book, video, person);
                await dbContext.SaveChangesAsync();
            });
        
            var route = $"/males/{person.Id}/relationships/favoriteContent";
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
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var favoriteContent = (await dbContext.Males
                        .Include(h => h.HumanContentItems)
                            .ThenInclude(hp => hp.Content)
                        .SingleAsync(h => h.Id.Equals(person.Id)))
                    .HumanContentItems.Select(hp => hp.Content).ToList();
        
                favoriteContent.Should().HaveCount(2);
                favoriteContent.Should().ContainSingle(h => h is Book);
                favoriteContent.Should().ContainSingle(h => h is Video);
            });
        }
    }
}
