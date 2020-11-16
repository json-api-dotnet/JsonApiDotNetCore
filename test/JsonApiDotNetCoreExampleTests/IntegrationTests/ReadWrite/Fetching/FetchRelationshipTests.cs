using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ReadWrite.Fetching
{
    public sealed class FetchRelationshipTests
        : IClassFixture<IntegrationTestContext<TestableStartup<WriteDbContext>, WriteDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<WriteDbContext>, WriteDbContext> _testContext;
        private readonly WriteFakers _fakers = new WriteFakers();

        public FetchRelationshipTests(IntegrationTestContext<TestableStartup<WriteDbContext>, WriteDbContext> testContext)
        {
            _testContext = testContext;
        }

        [Fact]
        public async Task Can_get_HasOne_relationship()
        {
            var workItem = _fakers.WorkItem.Generate();
            workItem.Assignee = _fakers.UserAccount.Generate();
            
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(workItem);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/workItems/{workItem.StringId}/relationships/assignee";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Type.Should().Be("userAccounts");
            responseDocument.SingleData.Id.Should().Be(workItem.Assignee.StringId);
            responseDocument.SingleData.Attributes.Should().BeNull();
        }

        [Fact]
        public async Task Can_get_empty_HasOne_relationship()
        {
            var workItem = _fakers.WorkItem.Generate();
            
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(workItem);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/workItems/{workItem.StringId}/relationships/assignee";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.Should().BeNull();
        }

        [Fact]
        public async Task Can_get_HasMany_relationship()
        {
            // Arrange
            var userAccount = _fakers.UserAccount.Generate();
            userAccount.AssignedItems = _fakers.WorkItem.Generate(2).ToHashSet();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.UserAccounts.Add(userAccount);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/userAccounts/{userAccount.StringId}/relationships/assignedItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(2);

            var item1 = responseDocument.ManyData.Single(resource => resource.Id == userAccount.AssignedItems.ElementAt(0).StringId);
            item1.Type.Should().Be("workItems");
            item1.Attributes.Should().BeNull();

            var item2 = responseDocument.ManyData.Single(resource => resource.Id == userAccount.AssignedItems.ElementAt(1).StringId);
            item2.Type.Should().Be("workItems");
            item2.Attributes.Should().BeNull();
        }

        [Fact]
        public async Task Can_get_empty_HasMany_relationship()
        {
            // Arrange
            var userAccount = _fakers.UserAccount.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.UserAccounts.Add(userAccount);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/userAccounts/{userAccount.StringId}/relationships/assignedItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().BeEmpty();
        }

        [Fact]
        public async Task Can_get_HasManyThrough_relationship()
        {
            // Arrange
            var workItem = _fakers.WorkItem.Generate();
            workItem.WorkItemTags = new List<WorkItemTag>
            {
                new WorkItemTag
                {
                    Tag = _fakers.WorkTags.Generate()
                },
                new WorkItemTag
                {
                    Tag = _fakers.WorkTags.Generate()
                }
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(workItem);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/workItems/{workItem.StringId}/relationships/tags";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(2);

            var item1 = responseDocument.ManyData.Single(resource => resource.Id == workItem.WorkItemTags.ElementAt(0).Tag.StringId);
            item1.Type.Should().Be("workTags");
            item1.Attributes.Should().BeNull();
            
            var item2 = responseDocument.ManyData.Single(resource => resource.Id == workItem.WorkItemTags.ElementAt(1).Tag.StringId);
            item2.Type.Should().Be("workTags");
            item2.Attributes.Should().BeNull();
        }

        [Fact]
        public async Task Can_get_empty_HasManyThrough_relationship()
        {
            // Arrange
            var workItem = _fakers.WorkItem.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(workItem);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/workItems/{workItem.StringId}/relationships/tags";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().BeEmpty();
        }

        [Fact]
        public async Task Cannot_get_relationship_for_unknown_primary_type()
        {
            var route = "/doesNotExist/99999999/relationships/assignee";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Should().BeEmpty();
        }

        [Fact]
        public async Task Cannot_get_relationship_for_unknown_primary_ID()
        {
            var route = "/workItems/99999999/relationships/assignee";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.NotFound);
            responseDocument.Errors[0].Title.Should().Be("The requested resource does not exist.");
            responseDocument.Errors[0].Detail.Should().Be("Resource of type 'workItems' with ID '99999999' does not exist.");
        }

        [Fact]
        public async Task Cannot_get_relationship_for_unknown_relationship_type()
        {
            var workItem = _fakers.WorkItem.Generate();
            
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(workItem);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/workItems/{workItem.StringId}/relationships/doesNotExist";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.NotFound);
            responseDocument.Errors[0].Title.Should().Be("The requested relationship does not exist.");
            responseDocument.Errors[0].Detail.Should().Be("Resource of type 'workItems' does not contain a relationship named 'doesNotExist'.");
        }
    }
}
