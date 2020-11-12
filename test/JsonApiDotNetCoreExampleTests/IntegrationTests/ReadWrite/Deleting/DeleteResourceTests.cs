using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ReadWrite.Deleting
{
    public sealed class DeleteResourceTests
        : IClassFixture<IntegrationTestContext<TestableStartup<WriteDbContext>, WriteDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<WriteDbContext>, WriteDbContext> _testContext;
        private readonly WriteFakers _fakers = new WriteFakers();

        public DeleteResourceTests(IntegrationTestContext<TestableStartup<WriteDbContext>, WriteDbContext> testContext)
        {
            _testContext = testContext;
        }

        [Fact]
        public async Task Can_delete_existing_resource()
        {
            // Arrange
            var existingWorkItem = _fakers.WorkItem.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(existingWorkItem);
                await dbContext.SaveChangesAsync();
            });

            var route = "/workItems/" + existingWorkItem.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var workItemsInDatabase = await dbContext.WorkItems
                    .FirstOrDefaultAsync(workItem => workItem.Id == existingWorkItem.Id);

                workItemsInDatabase.Should().BeNull();
            });
        }

        [Fact]
        public async Task Cannot_delete_missing_resource()
        {
            // Arrange
            var route = "/workItems/99999999";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteDeleteAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.NotFound);
            responseDocument.Errors[0].Title.Should().Be("The requested resource does not exist.");
            responseDocument.Errors[0].Detail.Should().Be("Resource of type 'workItems' with ID '99999999' does not exist.");
        }

        [Fact]
        public async Task Can_delete_resource_with_OneToOne_relationship_from_dependent_side()
        {
            // Arrange
            var existingColor = _fakers.RgbColor.Generate();
            existingColor.Group = _fakers.WorkItemGroup.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.RgbColors.Add(existingColor);
                await dbContext.SaveChangesAsync();
            });

            var route = "/rgbColors/" + existingColor.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var colorsInDatabase = await dbContext.RgbColors
                    .FirstOrDefaultAsync(color => color.Id == existingColor.Id);
                
                colorsInDatabase.Should().BeNull();

                var groupInDatabase = await dbContext.Groups
                    .FirstAsync(group => group.Id == existingColor.Group.Id);

                groupInDatabase.Color.Should().BeNull();
            });
        }

        [Fact]
        public async Task Can_delete_existing_resource_with_OneToOne_relationship_from_principal_side()
        {
            // Arrange
            var existingGroup = _fakers.WorkItemGroup.Generate();
            existingGroup.Color = _fakers.RgbColor.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Groups.Add(existingGroup);
                await dbContext.SaveChangesAsync();
            });

            var route = "/workItemGroups/" + existingGroup.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var groupsInDatabase = await dbContext.Groups
                    .FirstOrDefaultAsync(group => group.Id == existingGroup.Id);

                groupsInDatabase.Should().BeNull();

                var colorInDatabase = await dbContext.RgbColors
                    .FirstOrDefaultAsync(color => color.Id == existingGroup.Color.Id);

                colorInDatabase.Should().NotBeNull();
                colorInDatabase.Group.Should().BeNull();
            });
        }

        [Fact]
        public async Task Can_delete_existing_resource_with_HasMany_relationship()
        {
            // Arrange
            var existingWorkItem = _fakers.WorkItem.Generate();
            existingWorkItem.Subscribers = _fakers.UserAccount.Generate(2).ToHashSet();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(existingWorkItem);
                await dbContext.SaveChangesAsync();
            });

            var route = "/workItems/" + existingWorkItem.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var workItemInDatabase = await dbContext.WorkItems
                    .FirstOrDefaultAsync(workItem => workItem.Id == existingWorkItem.Id);

                workItemInDatabase.Should().BeNull();

                var userAccountsInDatabase = await dbContext.UserAccounts.ToListAsync();

                userAccountsInDatabase.Should().ContainSingle(userAccount => userAccount.Id == existingWorkItem.Subscribers.ElementAt(0).Id);
                userAccountsInDatabase.Should().ContainSingle(userAccount => userAccount.Id == existingWorkItem.Subscribers.ElementAt(1).Id);
            });
        }

        [Fact]
        public async Task Can_delete_resource_with_HasManyThrough_relationship()
        {
           // Arrange
           var existingWorkItemTag = new WorkItemTag
            {
                Item = _fakers.WorkItem.Generate(),
                Tag = _fakers.WorkTags.Generate()
            };

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkItemTags.Add(existingWorkItemTag);
                await dbContext.SaveChangesAsync();
            });

            var route = "/workItems/" + existingWorkItemTag.Item.StringId;

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var workItemsInDatabase = await dbContext.WorkItems
                    .FirstOrDefaultAsync(workItem => workItem.Id == existingWorkItemTag.Item.Id);

                workItemsInDatabase.Should().BeNull();

                var workItemTagsInDatabase = await dbContext.WorkItemTags
                    .FirstOrDefaultAsync(workItemTag => workItemTag.Item.Id == existingWorkItemTag.Item.Id);

                workItemTagsInDatabase.Should().BeNull();
            });
        }
    }
}
