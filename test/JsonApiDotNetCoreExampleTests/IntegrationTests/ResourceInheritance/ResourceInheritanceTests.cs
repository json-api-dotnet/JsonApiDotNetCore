using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance
{
    public sealed class ResourceInheritanceTests : IClassFixture<IntegrationTestContext<TestableStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> _testContext;

        public ResourceInheritanceTests(IntegrationTestContext<TestableStartup<ResourceInheritanceDbContext>, ResourceInheritanceDbContext> testContext)
        {
            _testContext = testContext;

            testContext.ConfigureServicesAfterStartup(services =>
            {

            });
        }

        [Theory]
        [InlineData("students", 0)]
        [InlineData("teachers", 1)]
        public async Task Can_create_article_with_relationship_that_has_inheritance(string type, int index)
        {
            // Arrange
            var persons  = new List<Person>
            {
                new Student()
                {
                    InheritedProperty = "Student",
                    StudentProperty = "This is a student"
                },
                new Teacher()
                {
                    InheritedProperty = "Teacher",
                    TeacherProperty = "This is a teacher"
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Article>();
                await dbContext.ClearTableAsync<Person>();
                await dbContext.Persons.AddRangeAsync(persons);

                await dbContext.SaveChangesAsync();
            });

            var route = "/articles?include=author";
            var requestBody = new
            {
                data = new
                {
                    type = "articles",
                    attributes = new Dictionary<string, object>
                    {
                        { "title", "JsonApiDotNetCore" }
                    },
                    relationships = new Dictionary<string, object>
                    {
                        { "author", new
                            {
                                data = new { type, id = persons[index].StringId  }
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
            responseDocument.SingleData.Relationships["author"].Should().NotBeNull();
            responseDocument.SingleData.Relationships["author"].SingleData.Type.Should().Be(type);
            responseDocument.SingleData.Relationships["author"].SingleData.Id.Should().Be(persons[index].StringId);
        }
        
        [Fact]
        public async Task Blaat()
        {
            // Arrange
            var persons  = new List<Person>
            {
                new Student()
                {
                    InheritedProperty = "Student",
                    StudentProperty = "This is a student"
                },
                new Teacher()
                {
                    InheritedProperty = "Teacher",
                    TeacherProperty = "This is a teacher"
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Article>();
                await dbContext.ClearTableAsync<Person>();
                await dbContext.Persons.AddRangeAsync(persons);

                await dbContext.SaveChangesAsync();
            });

            var route = "/articles?include=reviewers";
            var requestBody = new
            {
                data = new
                {
                    type = "articles",
                    attributes = new Dictionary<string, object>
                    {
                        { "title", "JsonApiDotNetCore" }
                    },
                    relationships = new Dictionary<string, object>
                    {
                        { "reviewers", new
                            { data = new []
                                {
                                    new { type = "students", id = persons[0].StringId },
                                    new { type = "teachers", id = persons[1].StringId }
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
            responseDocument.SingleData.Relationships["reviewers"].ManyData.Should().HaveCount(2);
            responseDocument.SingleData.Relationships["reviewers"].ManyData[0].Type.Should().Be("students");
            responseDocument.SingleData.Relationships["reviewers"].ManyData[0].Id.Should().Be(persons[0].StringId);
            responseDocument.SingleData.Relationships["reviewers"].ManyData[1].Type.Should().Be("teachers");
            responseDocument.SingleData.Relationships["reviewers"].ManyData[1].Id.Should().Be(persons[1].StringId);
        }
    }
}
