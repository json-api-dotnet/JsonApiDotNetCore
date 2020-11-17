using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExample;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Links
{
    public sealed class RelativeLinksWithNamespaceTests
        : IClassFixture<IntegrationTestContext<Startup, AppDbContext>>
    {
        private readonly IntegrationTestContext<Startup, AppDbContext> _testContext;

        public RelativeLinksWithNamespaceTests(IntegrationTestContext<Startup, AppDbContext> testContext)
        {
            _testContext = testContext;

            var options = (JsonApiOptions) testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.Namespace = "api/v1";
            options.UseRelativeLinks = true;
            options.DefaultPageSize = new PageSize(10);
            options.IncludeTotalResourceCount = true;
        }

        [Fact]
        public async Task Get_primary_resource_by_ID_returns_links()
        {
            // Arrange
            var person = new Person();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.People.Add(person);
                await dbContext.SaveChangesAsync();
            });

            var route = "/api/v1/people/" + person.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Links.Self.Should().Be($"/api/v1/people/{person.StringId}");
            responseDocument.Links.Related.Should().BeNull();
            responseDocument.Links.First.Should().BeNull();
            responseDocument.Links.Last.Should().BeNull();
            responseDocument.Links.Prev.Should().BeNull();
            responseDocument.Links.Next.Should().BeNull();

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Links.Self.Should().Be($"/api/v1/people/{person.StringId}");

            responseDocument.SingleData.Relationships["todoItems"].Links.Self.Should().Be($"/api/v1/people/{person.StringId}/relationships/todoItems");
            responseDocument.SingleData.Relationships["todoItems"].Links.Related.Should().Be($"/api/v1/people/{person.StringId}/todoItems");
        }

        [Fact]
        public async Task Get_primary_resources_with_include_returns_links()
        {
            // Arrange
            var person = new Person
            {
                TodoItems = new HashSet<TodoItem>
                {
                    new TodoItem()
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Person>();
                dbContext.People.Add(person);
                await dbContext.SaveChangesAsync();
            });

            var route = "/api/v1/people?include=todoItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Links.Self.Should().Be("/api/v1/people?include=todoItems");
            responseDocument.Links.Related.Should().BeNull();
            responseDocument.Links.First.Should().Be("/api/v1/people?include=todoItems");
            responseDocument.Links.Last.Should().Be("/api/v1/people?include=todoItems");
            responseDocument.Links.Prev.Should().BeNull();
            responseDocument.Links.Next.Should().BeNull();

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Links.Self.Should().Be($"/api/v1/people/{person.StringId}");

            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Links.Self.Should().Be($"/api/v1/todoItems/{person.TodoItems.ElementAt(0).StringId}");
        }

        [Fact]
        public async Task Get_HasMany_relationship_returns_links()
        {
            // Arrange
            var person = new Person
            {
                TodoItems = new HashSet<TodoItem>
                {
                    new TodoItem()
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Person>();
                dbContext.People.Add(person);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/api/v1/people/{person.StringId}/relationships/todoItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Links.Self.Should().Be($"/api/v1/people/{person.StringId}/relationships/todoItems");
            responseDocument.Links.Related.Should().Be($"/api/v1/people/{person.StringId}/todoItems");
            responseDocument.Links.First.Should().Be($"/api/v1/people/{person.StringId}/relationships/todoItems");
            responseDocument.Links.Last.Should().BeNull();
            responseDocument.Links.Prev.Should().BeNull();
            responseDocument.Links.Next.Should().BeNull();

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Links.Should().BeNull();
        }
    }
}
