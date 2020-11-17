using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ReadWrite.Fetching
{
    public sealed class FetchResourceTests
        : IClassFixture<IntegrationTestContext<TestableStartup<WriteDbContext>, WriteDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<WriteDbContext>, WriteDbContext> _testContext;
        private readonly WriteFakers _fakers = new WriteFakers();

        public FetchResourceTests(IntegrationTestContext<TestableStartup<WriteDbContext>, WriteDbContext> testContext)
        {
            _testContext = testContext;
        }

        [Fact]
        public async Task Can_get_primary_resources()
        {
            // Arrange
            var workItems = _fakers.WorkItem.Generate(2);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<WorkItem>();
                dbContext.WorkItems.AddRange(workItems);
                await dbContext.SaveChangesAsync();
            });

            var route = "/workItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(2);

            var item1 = responseDocument.ManyData.Single(resource => resource.Id == workItems[0].StringId);
            item1.Type.Should().Be("workItems");
            item1.Attributes["description"].Should().Be(workItems[0].Description);
            item1.Attributes["dueAt"].Should().BeCloseTo(workItems[0].DueAt);
            item1.Attributes["priority"].Should().Be(workItems[0].Priority.ToString("G"));
            
            var item2 = responseDocument.ManyData.Single(resource => resource.Id == workItems[1].StringId);
            item2.Type.Should().Be("workItems");
            item2.Attributes["description"].Should().Be(workItems[1].Description);
            item2.Attributes["dueAt"].Should().BeCloseTo(workItems[1].DueAt);
            item2.Attributes["priority"].Should().Be(workItems[1].Priority.ToString("G"));
        }

        [Fact]
        public async Task Cannot_get_primary_resources_for_unknown_type()
        {
            // Arrange
            var route = "/doesNotExist";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Should().BeEmpty();
        }

        [Fact]
        public async Task Can_get_primary_resource_by_ID()
        {
            // Arrange
            var workItem = _fakers.WorkItem.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(workItem);
                await dbContext.SaveChangesAsync();
            });

            var route = "/workItems/" + workItem.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Type.Should().Be("workItems");
            responseDocument.SingleData.Id.Should().Be(workItem.StringId);
            responseDocument.SingleData.Attributes["description"].Should().Be(workItem.Description);
            responseDocument.SingleData.Attributes["dueAt"].Should().BeCloseTo(workItem.DueAt);
            responseDocument.SingleData.Attributes["priority"].Should().Be(workItem.Priority.ToString("G"));
        }

        [Fact]
        public async Task Cannot_get_primary_resource_for_unknown_type()
        {
            // Arrange
            var route = "/doesNotExist/99999999";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Should().BeEmpty();
        }

        [Fact]
        public async Task Cannot_get_primary_resource_for_unknown_ID()
        {
            // Arrange
            var route = "/workItems/99999999";

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
        public async Task Can_get_secondary_HasOne_resource()
        {
            // Arrange
            var workItem = _fakers.WorkItem.Generate();
            workItem.Assignee = _fakers.UserAccount.Generate();
            
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(workItem);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/workItems/{workItem.StringId}/assignee";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Type.Should().Be("userAccounts");
            responseDocument.SingleData.Id.Should().Be(workItem.Assignee.StringId);
            responseDocument.SingleData.Attributes["firstName"].Should().Be(workItem.Assignee.FirstName);
            responseDocument.SingleData.Attributes["lastName"].Should().Be(workItem.Assignee.LastName);
        }

        [Fact]
        public async Task Can_get_unknown_secondary_HasOne_resource()
        {
            // Arrange
            var workItem = _fakers.WorkItem.Generate();
            
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(workItem);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/workItems/{workItem.StringId}/assignee";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.Should().BeNull();
        }

        [Fact]
        public async Task Can_get_secondary_HasMany_resources()
        {
            // Arrange
            var userAccount = _fakers.UserAccount.Generate();
            userAccount.AssignedItems = _fakers.WorkItem.Generate(2).ToHashSet();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.UserAccounts.Add(userAccount);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/userAccounts/{userAccount.StringId}/assignedItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(2);

            var item1 = responseDocument.ManyData.Single(resource => resource.Id == userAccount.AssignedItems.ElementAt(0).StringId);
            item1.Type.Should().Be("workItems");
            item1.Attributes["description"].Should().Be(userAccount.AssignedItems.ElementAt(0).Description);
            item1.Attributes["dueAt"].Should().BeCloseTo(userAccount.AssignedItems.ElementAt(0).DueAt);
            item1.Attributes["priority"].Should().Be(userAccount.AssignedItems.ElementAt(0).Priority.ToString("G"));
            
            var item2 = responseDocument.ManyData.Single(resource => resource.Id == userAccount.AssignedItems.ElementAt(1).StringId);
            item2.Type.Should().Be("workItems");
            item2.Attributes["description"].Should().Be(userAccount.AssignedItems.ElementAt(1).Description);
            item2.Attributes["dueAt"].Should().BeCloseTo(userAccount.AssignedItems.ElementAt(1).DueAt);
            item2.Attributes["priority"].Should().Be(userAccount.AssignedItems.ElementAt(1).Priority.ToString("G"));
        }

        [Fact]
        public async Task Can_get_unknown_secondary_HasMany_resource()
        {
            // Arrange
            var userAccount = _fakers.UserAccount.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.UserAccounts.Add(userAccount);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/userAccounts/{userAccount.StringId}/assignedItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().BeEmpty();
        }

        [Fact]
        public async Task Can_get_secondary_HasManyThrough_resources()
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

            var route = $"/workItems/{workItem.StringId}/tags";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(2);

            var item1 = responseDocument.ManyData.Single(resource => resource.Id == workItem.WorkItemTags.ElementAt(0).Tag.StringId);
            item1.Type.Should().Be("workTags");
            item1.Attributes["text"].Should().Be(workItem.WorkItemTags.ElementAt(0).Tag.Text);
            item1.Attributes["isBuiltIn"].Should().Be(workItem.WorkItemTags.ElementAt(0).Tag.IsBuiltIn);
            
            var item2 = responseDocument.ManyData.Single(resource => resource.Id == workItem.WorkItemTags.ElementAt(1).Tag.StringId);
            item2.Type.Should().Be("workTags");
            item2.Attributes["text"].Should().Be(workItem.WorkItemTags.ElementAt(1).Tag.Text);
            item2.Attributes["isBuiltIn"].Should().Be(workItem.WorkItemTags.ElementAt(1).Tag.IsBuiltIn);
        }

        [Fact]
        public async Task Can_get_unknown_secondary_HasManyThrough_resources()
        {
            // Arrange
            var workItem = _fakers.WorkItem.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(workItem);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/workItems/{workItem.StringId}/tags";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().BeEmpty();
        }

        [Fact]
        public async Task Cannot_get_secondary_resource_for_unknown_primary_type()
        {
            // Arrange
            var route = "/doesNotExist/99999999/assignee";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Should().BeEmpty();
        }

        [Fact]
        public async Task Cannot_get_secondary_resource_for_unknown_primary_ID()
        {
            // Arrange
            var route = "/workItems/99999999/assignee";

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
        public async Task Cannot_get_secondary_resource_for_unknown_secondary_type()
        {
            // Arrange
            var workItem = _fakers.WorkItem.Generate();
            workItem.Assignee = _fakers.UserAccount.Generate();
            
            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(workItem);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/workItems/{workItem.StringId}/doesNotExist";

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
