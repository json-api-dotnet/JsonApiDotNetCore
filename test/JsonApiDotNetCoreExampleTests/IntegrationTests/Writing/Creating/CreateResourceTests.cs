using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Writing.Creating
{
    public sealed class CreateResourceTests
        : IClassFixture<IntegrationTestContext<TestableStartup<WriteDbContext>, WriteDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<WriteDbContext>, WriteDbContext> _testContext;
        private readonly WriteFakers _fakers = new WriteFakers();

        public CreateResourceTests(IntegrationTestContext<TestableStartup<WriteDbContext>, WriteDbContext> testContext)
        {
            _testContext = testContext;

            var options = (JsonApiOptions) testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.UseRelativeLinks = false;
            options.AllowClientGeneratedIds = false;
        }

        [Fact]
        public async Task Sets_location_header_for_created_resource()
        {
            // Arrange
            var workItem = _fakers.WorkItem.Generate();

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    attributes = new
                    {
                        description = workItem.Description
                    }
                }
            };

            var route = "/workItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            var newWorkItemId = responseDocument.SingleData.Id;
            httpResponse.Headers.Location.Should().Be("/workItems/" + newWorkItemId);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Links.Self.Should().Be("http://localhost" + httpResponse.Headers.Location);
        }

        [Fact]
        public async Task Can_create_resource_with_int_ID()
        {
            // Arrange
            var workItem = _fakers.WorkItem.Generate();
            workItem.DueAt = null;

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    attributes = new
                    {
                        description = workItem.Description
                    }
                }
            };

            var route = "/workItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Type.Should().Be("workItems");
            responseDocument.SingleData.Attributes["description"].Should().Be(workItem.Description);
            responseDocument.SingleData.Attributes["dueAt"].Should().Be(workItem.DueAt);

            responseDocument.SingleData.Relationships.Should().NotBeEmpty();

            var newWorkItemId = responseDocument.SingleData.Id;
            newWorkItemId.Should().NotBeNullOrEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var workItemsInDatabase = await dbContext.WorkItems.ToListAsync();

                var newWorkItemInDatabase = workItemsInDatabase.Single(p => p.StringId == newWorkItemId);
                newWorkItemInDatabase.Description.Should().Be(workItem.Description);
                newWorkItemInDatabase.DueAt.Should().Be(workItem.DueAt);
            });

            var property = typeof(WorkItem).GetProperty(nameof(Identifiable.Id));
            property.Should().NotBeNull().And.Subject.PropertyType.Should().Be(typeof(int));
        }

        [Fact]
        public async Task Can_create_resource_with_long_ID()
        {
            // Arrange
            var userAccount = _fakers.UserAccount.Generate();

            var requestBody = new
            {
                data = new
                {
                    type = "userAccounts",
                    attributes = new
                    {
                        firstName = userAccount.FirstName,
                        lastName = userAccount.LastName
                    }
                }
            };

            var route = "/userAccounts";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Type.Should().Be("userAccounts");
            responseDocument.SingleData.Attributes["firstName"].Should().Be(userAccount.FirstName);
            responseDocument.SingleData.Attributes["lastName"].Should().Be(userAccount.LastName);

            responseDocument.SingleData.Relationships.Should().NotBeEmpty();

            var newUserAccountId = responseDocument.SingleData.Id;
            newUserAccountId.Should().NotBeNullOrEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var userAccountsInDatabase = await dbContext.UserAccounts.ToListAsync();

                var newUserAccountInDatabase = userAccountsInDatabase.Single(p => p.StringId == newUserAccountId);
                newUserAccountInDatabase.FirstName.Should().Be(userAccount.FirstName);
                newUserAccountInDatabase.LastName.Should().Be(userAccount.LastName);
            });

            var property = typeof(UserAccount).GetProperty(nameof(Identifiable.Id));
            property.Should().NotBeNull().And.Subject.PropertyType.Should().Be(typeof(long));
        }

        [Fact]
        public async Task Can_create_resource_with_guid_ID()
        {
            // Arrange
            var group = _fakers.WorkItemGroup.Generate();

            var requestBody = new
            {
                data = new
                {
                    type = "workItemGroups",
                    attributes = new
                    {
                        name = group.Name
                    }
                }
            };

            var route = "/workItemGroups";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Type.Should().Be("workItemGroups");
            responseDocument.SingleData.Attributes["name"].Should().Be(group.Name);

            responseDocument.SingleData.Relationships.Should().NotBeEmpty();

            var newGroupId = responseDocument.SingleData.Id;
            newGroupId.Should().NotBeNullOrEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var groupsInDatabase = await dbContext.Groups.ToListAsync();

                var newGroupInDatabase = groupsInDatabase.Single(p => p.StringId == newGroupId);
                newGroupInDatabase.Name.Should().Be(group.Name);
            });

            var property = typeof(WorkItemGroup).GetProperty(nameof(Identifiable.Id));
            property.Should().NotBeNull().And.Subject.PropertyType.Should().Be(typeof(Guid));
        }

        [Fact]
        public async Task Can_create_resource_without_attributes_or_relationships()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    attributes = new
                    {
                    },
                    relationship = new
                    {
                    }
                }
            };

            var route = "/workItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Type.Should().Be("workItems");
            responseDocument.SingleData.Attributes["description"].Should().BeNull();
            responseDocument.SingleData.Attributes["dueAt"].Should().BeNull();

            responseDocument.SingleData.Relationships.Should().NotBeEmpty();

            var newWorkItemId = responseDocument.SingleData.Id;
            newWorkItemId.Should().NotBeNullOrEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var workItemsInDatabase = await dbContext.WorkItems.ToListAsync();

                var newWorkItemInDatabase = workItemsInDatabase.Single(p => p.StringId == newWorkItemId);
                newWorkItemInDatabase.Description.Should().BeNull();
                newWorkItemInDatabase.DueAt.Should().BeNull();
            });
        }

        [Fact]
        public async Task Can_create_resource_with_unknown_attribute()
        {
            // Arrange
            var workItem = _fakers.WorkItem.Generate();

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    attributes = new
                    {
                        doesNotExist = "ignored",
                        description = workItem.Description
                    }
                }
            };

            var route = "/workItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Type.Should().Be("workItems");
            responseDocument.SingleData.Attributes["description"].Should().Be(workItem.Description);

            var newWorkItemId = responseDocument.SingleData.Id;
            newWorkItemId.Should().NotBeNullOrEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var workItemsInDatabase = await dbContext.WorkItems.ToListAsync();

                var newWorkItemInDatabase = workItemsInDatabase.Single(p => p.StringId == newWorkItemId);
                newWorkItemInDatabase.Description.Should().Be(workItem.Description);
            });
        }

        [Fact]
        public async Task Cannot_create_resource_with_client_generated_ID()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "rgbColors",
                    id = "#000000",
                    attributes = new
                    {
                        name = "Black"
                    }
                }
            };

            var route = "/rgbColors";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Forbidden);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.Forbidden);
            responseDocument.Errors[0].Title.Should().Be("Specifying the resource ID in POST requests is not allowed.");
            responseDocument.Errors[0].Detail.Should().BeNull();
            responseDocument.Errors[0].Source.Pointer.Should().Be("/data/id");
        }

        [Fact]
        public async Task Cannot_create_resource_for_missing_type()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    attributes = new
                    {
                    }
                }
            };

            var route = "/workItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: Request body must include 'type' element.");
            responseDocument.Errors[0].Detail.Should().StartWith("Expected 'type' element in 'data' element. - Request body: <<");
        }

        [Fact]
        public async Task Cannot_create_resource_for_unknown_type()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "doesNotExist",
                    attributes = new
                    {
                    }
                }
            };

            var route = "/workItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: Request body includes unknown resource type.");
            responseDocument.Errors[0].Detail.Should().StartWith("Resource of type 'doesNotExist' does not exist.");
            responseDocument.Errors[0].Detail.Should().Contain("Request body: <<");
        }

        [Fact]
        public async Task Cannot_create_resource_on_unknown_resource_type_in_url()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    attributes = new
                    {
                    }
                }
            };

            var route = "/doesNotExist";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Should().BeEmpty();
        }

        [Fact]
        public async Task Cannot_create_resource_with_blocked_attribute()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    attributes = new
                    {
                        concurrencyToken = "274E1D9A-91BE-4A42-B648-CA75E8B2945E"
                    }
                }
            };

            var route = "/workItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: Assigning to the requested attribute is not allowed.");
            responseDocument.Errors[0].Detail.Should().StartWith("Assigning to 'concurrencyToken' is not allowed. - Request body:");
        }

        [Fact]
        public async Task Cannot_create_resource_with_readonly_attribute()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "workItemGroups",
                    attributes = new
                    {
                        concurrencyToken = "274E1D9A-91BE-4A42-B648-CA75E8B2945E"
                    }
                }
            };

            var route = "/workItemGroups";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body.");
            responseDocument.Errors[0].Detail.Should().StartWith("Property 'WorkItemGroup.ConcurrencyToken' is read-only. - Request body:");
        }

        [Fact]
        public async Task Cannot_create_resource_for_broken_JSON_request_body()
        {
            // Arrange
            var requestBody = "{ \"data\" {";

            var route = "/workItemGroups";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body.");
            responseDocument.Errors[0].Detail.Should().StartWith("Invalid character after parsing");
        }
    }
}
