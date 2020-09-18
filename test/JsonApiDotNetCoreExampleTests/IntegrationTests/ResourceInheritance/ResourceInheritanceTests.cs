using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance
{
    public sealed class ResourceInheritanceTests : IClassFixture<IntegrationTestContext<TestableStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> _testContext;
        private readonly List<Person> _persons = new List<Person> { new Male(), new Female() };
        private readonly Placeholder _placeholder = new Placeholder();

        public ResourceInheritanceTests(IntegrationTestContext<TestableStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> testContext)
        {
            _testContext = testContext;
        }

        [Theory]
        [InlineData("males", 0)]
        [InlineData("females", 1)]
        public async Task Can_create_resource_with_one_to_one_relationship_that_has_inheritance(string type, int index)
        {
            // Arrange
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<PlaceholderPerson>();
                await dbContext.ClearTableAsync<Person>();
                await dbContext.ClearTableAsync<Placeholder>();
                await dbContext.Persons.AddRangeAsync(_persons);

                await dbContext.SaveChangesAsync();
            });

            var route = "/placeholders?include=oneToOnePerson";
            var requestBody = new
            {
                data = new
                {
                    type = "placeholders",
                    relationships = new Dictionary<string, object>
                    {
                        { "oneToOnePerson", new
                            {
                                data = new { type, id = _persons[index].StringId  }
                            } 
                        }
                    }
                }
            };

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);
            
            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);
            
            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Relationships["oneToOnePerson"].Should().NotBeNull();
            responseDocument.SingleData.Relationships["oneToOnePerson"].SingleData.Type.Should().Be(type);
            responseDocument.SingleData.Relationships["oneToOnePerson"].SingleData.Id.Should().Be(_persons[index].StringId);
        }

        [Theory]
        [InlineData("males", 0)]
        [InlineData("females", 1)]
        public async Task Can_patch_one_to_one_relationship_that_has_inheritance_through_relationship_endpoint(string type, int index)
        {
            // Arrange
            var expectedType = _persons[index].GetType();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<PlaceholderPerson>();
                await dbContext.ClearTableAsync<Person>();
                await dbContext.ClearTableAsync<Placeholder>();
                await dbContext.Persons.AddRangeAsync(_persons);
                await dbContext.Placeholders.AddAsync(_placeholder);

                await dbContext.SaveChangesAsync();
            });


            var route = $"/placeholders/{_placeholder.Id}/relationships/oneToOnePerson";

            var requestBody = new
            {
                data = new { type, id = _persons[index].StringId  }
            };

            // Act
            var (httpResponse, _) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);
            
            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
            
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.SaveChangesAsync();
                var assertPlaceholder = await dbContext.Placeholders
                    .Include(p => p.OneToOnePerson)
                    .Where(p => p.Id.Equals(_placeholder.Id)).FirstAsync();

                assertPlaceholder.OneToOnePerson.GetType().Should().Be(expectedType);
            });
        }

        
        [Fact]
        public async Task Can_create_resource_with_one_to_many_relationship_that_has_inheritance()
        {
            // Arrange
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<PlaceholderPerson>();
                await dbContext.ClearTableAsync<Person>();
                await dbContext.ClearTableAsync<Placeholder>();
                await dbContext.Persons.AddRangeAsync(_persons);

                await dbContext.SaveChangesAsync();
            });

            var route = "/placeholders?include=oneToManyPersons";
            var requestBody = new
            {
                data = new
                {
                    type = "placeholders",
                    relationships = new Dictionary<string, object>
                    {
                        { "oneToManyPersons", new
                            { data = new []
                                {
                                    new { type = "males", id = _persons[0].StringId },
                                    new { type = "females", id = _persons[1].StringId }
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
            
            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Relationships["oneToManyPersons"].ManyData.Should().HaveCount(2);
            responseDocument.SingleData.Relationships["oneToManyPersons"].ManyData[0].Type.Should().Be("males");
            responseDocument.SingleData.Relationships["oneToManyPersons"].ManyData[0].Id.Should().Be(_persons[0].StringId);
            responseDocument.SingleData.Relationships["oneToManyPersons"].ManyData[1].Type.Should().Be("females");
            responseDocument.SingleData.Relationships["oneToManyPersons"].ManyData[1].Id.Should().Be(_persons[1].StringId);
        }
        
        [Fact]
        public async Task Can_patch_one_to_many_relationship_that_has_inheritance_through_relationship_endpoint()
        {
            // Arrange            
            
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<PlaceholderPerson>();
                await dbContext.ClearTableAsync<Person>();
                await dbContext.ClearTableAsync<Placeholder>();
                await dbContext.Persons.AddRangeAsync(_persons);
                await dbContext.Placeholders.AddAsync(_placeholder);

                await dbContext.SaveChangesAsync();
            });

            var route = $"/placeholders/{_placeholder.Id}/relationships/oneToManyPersons";
            var requestBody = new
            {
                data = new []
                {
                    new { type = "males", id = _persons[0].StringId },
                    new { type = "females", id = _persons[1].StringId }
                }
            };

            // Act
            var (httpResponse, _) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);
            
            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
            
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.SaveChangesAsync();
                var assertPlaceholder = await dbContext.Placeholders
                    .Include(p => p.OneToManyPersons)
                    .Where(p => p.Id.Equals(_placeholder.Id)).FirstAsync();

                assertPlaceholder.OneToManyPersons.Should().HaveCount(2);
                assertPlaceholder.OneToManyPersons.Should().ContainSingle(p => p is Male);
                assertPlaceholder.OneToManyPersons.Should().ContainSingle(p => p is Female);
            });
        }
        
        [Fact]
        public async Task Can_create_resource_with_many_to_many_relationship_that_has_inheritance()
        {
            // Arrange
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<PlaceholderPerson>();
                await dbContext.ClearTableAsync<Person>();
                await dbContext.ClearTableAsync<Placeholder>();
                await dbContext.Persons.AddRangeAsync(_persons);

                await dbContext.SaveChangesAsync();
            });

            var route = "/placeholders?include=manyToManyPersons";
            var requestBody = new
            {
                data = new
                {
                    type = "placeholders",
                    relationships = new Dictionary<string, object>
                    {
                        { "manyToManyPersons", new
                            { data = new []
                                {
                                    new { type = "males", id = _persons[0].StringId },
                                    new { type = "females", id = _persons[1].StringId }
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
            
            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Relationships["manyToManyPersons"].ManyData.Should().HaveCount(2);
            responseDocument.SingleData.Relationships["manyToManyPersons"].ManyData[0].Type.Should().Be("males");
            responseDocument.SingleData.Relationships["manyToManyPersons"].ManyData[0].Id.Should().Be(_persons[0].StringId);
            responseDocument.SingleData.Relationships["manyToManyPersons"].ManyData[1].Type.Should().Be("females");
            responseDocument.SingleData.Relationships["manyToManyPersons"].ManyData[1].Id.Should().Be(_persons[1].StringId);
        }
        
        [Fact]
        public async Task Can_patch_many_to_many_relationship_that_has_inheritance_through_relationship_endpoint()
        {
            // Arrange
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<PlaceholderPerson>();
                await dbContext.ClearTableAsync<Person>();
                await dbContext.ClearTableAsync<Placeholder>();
                await dbContext.Persons.AddRangeAsync(_persons);
                await dbContext.Placeholders.AddAsync(_placeholder);

                await dbContext.SaveChangesAsync();
            });

            var route = $"/placeholders/{_placeholder.Id}/relationships/manyToManyPersons";
            var requestBody = new
            {
                data = new []
                {
                    new { type = "males", id = _persons[0].StringId },
                    new { type = "females", id = _persons[1].StringId }
                }
            };

            // Act
            var (httpResponse, _) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);
            
            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
            
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.SaveChangesAsync();
                var manyToManyPersons = (await dbContext.Placeholders
                    .Include(p => p.PlaceholderPersons)
                        .ThenInclude(pp => pp.Person)
                    .Where(p => p.Id.Equals(_placeholder.Id)).FirstAsync())
                    .PlaceholderPersons.Select(pp => pp.Person).ToList();
                
                manyToManyPersons.Should().HaveCount(2);
                manyToManyPersons.Should().ContainSingle(p => p is Male);
                manyToManyPersons.Should().ContainSingle(p => p is Female);
            });
        }
    }
}
