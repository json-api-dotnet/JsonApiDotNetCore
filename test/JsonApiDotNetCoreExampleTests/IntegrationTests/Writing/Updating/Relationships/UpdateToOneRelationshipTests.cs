using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Writing.Updating.Relationships
{
    // TODO: consider using <resourceName> instead of existing<ResourceName>
    //     - understandable without while not as verbose, less = more
    //     - in line with what we had/have

    // TODO: Consider using abbreviations instead of full parameter names in lambdas
    //    - in line with what we had
    //    - more readable because less verbose
    public sealed class UpdateToOneRelationshipTests
        : IClassFixture<IntegrationTestContext<TestableStartup<WriteDbContext>, WriteDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<WriteDbContext>, WriteDbContext> _testContext;
        private readonly WriteFakers _fakers = new WriteFakers();

        public UpdateToOneRelationshipTests(IntegrationTestContext<TestableStartup<WriteDbContext>, WriteDbContext> testContext)
        {
            _testContext = testContext;
        }
 
        [Fact]
        public async Task Can_clear_HasOne_relationship()
        {
            // Arrange
            var existingWorkItem = _fakers.WorkItem.Generate();
            existingWorkItem.AssignedTo = _fakers.UserAccount.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(existingWorkItem);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = (object)null
            };

            var route = $"/workItems/{existingWorkItem.StringId}/relationships/assignedTo";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var workItemInDatabase = await dbContext.WorkItems
                    .Include(workItem => workItem.AssignedTo)
                    .FirstAsync(workItem => workItem.Id == existingWorkItem.Id);

                workItemInDatabase.AssignedTo.Should().BeNull();

                // TODO: When checking if workItemInDatabase.AssignedTo is null, there is no need to also check that userAccountInDatabase.AssignedItems is empty.
                var userAccountInDatabase = await dbContext.UserAccounts
                    .Include(userAccount => userAccount.AssignedItems)
                    .FirstOrDefaultAsync(userAccount => userAccount.Id == existingWorkItem.AssignedTo.Id);

                userAccountInDatabase.Should().NotBeNull();
                userAccountInDatabase.AssignedItems.Should().BeEmpty();
            });
        }

        [Fact]
        public async Task Can_create_OneToOne_relationship_from_dependent_side()
        {
            // Arrange
            var existingGroup = _fakers.WorkItemGroup.Generate();
            existingGroup.Color = _fakers.RgbColor.Generate();
            
            var existingColor = _fakers.RgbColor.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddRange(existingGroup, existingColor);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "workItemGroups",
                    id = existingGroup.StringId
                }
            };

            var route = $"/rgbColors/{existingColor.StringId}/relationships/group";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var colorsInDatabase = await dbContext.RgbColors
                    .Include(rgbColor => rgbColor.Group)
                    .ToListAsync();

                // TODO: Redundant: given that we're working with a OneToOne relationship, if colorInDatabase2 is assigned to existingGroup
                // then it CANNOT be associated with colorInDatabase1 any more. With this double assertion we're merely
                // verifying that EF Core knows how to deals with relationships correctly.
                var colorInDatabase1 = colorsInDatabase.Single(p => p.Id == existingGroup.Color.Id);
                colorInDatabase1.Group.Should().BeNull();

                var colorInDatabase2 = colorsInDatabase.Single(p => p.Id == existingColor.Id);
                colorInDatabase2.Group.Should().NotBeNull();
                colorInDatabase2.Group.Id.Should().Be(existingGroup.Id);
            });
        }

        [Fact]
        public async Task Can_replace_OneToOne_relationship_from_principal_side()
        {
            // Arrange
            var existingGroups = _fakers.WorkItemGroup.Generate(2);
            existingGroups[0].Color = _fakers.RgbColor.Generate();
            existingGroups[1].Color = _fakers.RgbColor.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Groups.AddRange(existingGroups);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "rgbColors",
                    id = existingGroups[0].Color.StringId
                }
            };

            var route = $"/workItemGroups/{existingGroups[1].StringId}/relationships/color";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var groupsInDatabase = await dbContext.Groups
                    .Include(group => group.Color)
                    .ToListAsync();
                
                var groupInDatabase1 = groupsInDatabase.Single(p => p.Id == existingGroups[0].Id);
                groupInDatabase1.Color.Should().BeNull();

                var groupInDatabase2 = groupsInDatabase.Single(p => p.Id == existingGroups[1].Id);
                groupInDatabase2.Color.Should().NotBeNull();
                groupInDatabase2.Color.Id.Should().Be(existingGroups[0].Color.Id);

                var colorsInDatabase = await dbContext.RgbColors
                    .Include(color => color.Group)
                    .ToListAsync();

                var colorInDatabase1 = colorsInDatabase.Single(color => color.Id == existingGroups[0].Color.Id);
                colorInDatabase1.Group.Should().NotBeNull();
                colorInDatabase1.Group.Id.Should().Be(existingGroups[1].Id);

                var colorInDatabase2 = colorsInDatabase.Single(color => color.Id == existingGroups[1].Color.Id);
                colorInDatabase2.Group.Should().BeNull();
            });
        }

        [Fact]
        public async Task Can_replace_ManyToOne_relationship()
        {
            // Arrange
            var existingUserAccounts = _fakers.UserAccount.Generate(2);
            existingUserAccounts[0].AssignedItems = _fakers.WorkItem.Generate(2).ToHashSet();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.UserAccounts.AddRange(existingUserAccounts);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "userAccounts",
                    id = existingUserAccounts[1].StringId
                }
            };

            var route = $"/workItems/{existingUserAccounts[0].AssignedItems.ElementAt(1).StringId}/relationships/assignedTo";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var workItemsInDatabase = await dbContext.WorkItems
                    .Include(workItem => workItem.AssignedTo)
                    .ToListAsync();

                var workItemInDatabase2 = workItemsInDatabase.Single(p => p.Id == existingUserAccounts[0].AssignedItems.ElementAt(1).Id);
                workItemInDatabase2.AssignedTo.Should().NotBeNull();
                workItemInDatabase2.AssignedTo.Id.Should().Be(existingUserAccounts[1].Id);

                var userAccountsInDatabase = await dbContext.UserAccounts
                    .Include(userAccount => userAccount.AssignedItems)
                    .ToListAsync();

                var userAccountInDatabase1 = userAccountsInDatabase.Single(userAccount => userAccount.Id == existingUserAccounts[0].Id);
                userAccountInDatabase1.AssignedItems.Should().HaveCount(1);
                userAccountInDatabase1.AssignedItems.Single().Id.Should().Be(existingUserAccounts[0].AssignedItems.ElementAt(0).Id);

                var userAccountInDatabase2 = userAccountsInDatabase.Single(userAccount => userAccount.Id == existingUserAccounts[1].Id);
                userAccountInDatabase2.AssignedItems.Should().HaveCount(1);
                userAccountInDatabase2.AssignedItems.Single().Id.Should().Be(existingUserAccounts[0].AssignedItems.ElementAt(1).Id);
            });
        }

        // TODO: Consider moving to BaseDocumentParserTests
        [Fact]
        public async Task Cannot_create_for_missing_type()
        {
            // Arrange
            var existingWorkItem = _fakers.WorkItem.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(existingWorkItem);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    id = 99999999
                }
            };

            var route = $"/workItems/{existingWorkItem.StringId}/relationships/assignedTo";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: Request body must include 'type' element.");
            responseDocument.Errors[0].Detail.Should().StartWith("Expected 'type' element in 'data' element. - Request body: <<");
        }

        // TODO: Consider moving to BaseDocumentParserTests
        [Fact]
        public async Task Cannot_create_for_unknown_type()
        {
            // Arrange
            var existingWorkItem = _fakers.WorkItem.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(existingWorkItem);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "doesNotExist",
                    id = 99999999
                }
            };

            var route = $"/workItems/{existingWorkItem.StringId}/relationships/assignedTo";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: Request body includes unknown resource type.");
            responseDocument.Errors[0].Detail.Should().StartWith("Resource of type 'doesNotExist' does not exist. - Request body: <<");
        }

        // TODO: Consider moving to RequestDeserializerTests
        [Fact]
        public async Task Cannot_create_for_missing_ID()
        {
            // Arrange
            var existingWorkItem = _fakers.WorkItem.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(existingWorkItem);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "userAccounts"
                }
            };

            var route = $"/workItems/{existingWorkItem.StringId}/relationships/assignedTo";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: Request body must include 'id' element.");
            responseDocument.Errors[0].Detail.Should().StartWith("Expected 'id' element in 'data' element. - Request body: <<");
        }

        [Fact]
        public async Task Cannot_create_with_unknown_ID()
        {
            // Arrange
            var existingWorkItem = _fakers.WorkItem.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(existingWorkItem);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "userAccounts",
                    id = 99999999
                }
            };

            var route = $"/workItems/{existingWorkItem.StringId}/relationships/assignedTo";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.NotFound);
            responseDocument.Errors[0].Title.Should().Be("A resource being assigned to a relationship does not exist.");
            responseDocument.Errors[0].Detail.Should().Be("Resource of type 'userAccounts' with ID '99999999' being assigned to relationship 'assignedTo' does not exist.");
        }

        // TODO: This is a very general 404 test which is not exclusive to this or any of the other endpoints where it is duplicated.
        [Fact]
        public async Task Cannot_create_on_unknown_resource_type_in_url()
        {
            // Arrange
            var existingWorkItem = _fakers.WorkItem.Generate();
            var existingUserAccount = _fakers.UserAccount.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddRange(existingWorkItem, existingUserAccount);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "userAccounts",
                    id = existingUserAccount.StringId
                }
            };

            var route = $"/doesNotExist/{existingWorkItem.StringId}/relationships/assignedTo";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Should().BeEmpty();
        }

        [Fact]
        public async Task Cannot_create_on_unknown_resource_ID_in_url()
        {
            // Arrange
            var existingUserAccount = _fakers.UserAccount.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.UserAccounts.Add(existingUserAccount);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "userAccounts",
                    id = existingUserAccount.StringId
                }
            };

            var route = "/workItems/99999999/relationships/assignedTo";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.NotFound);
            responseDocument.Errors[0].Title.Should().Be("The requested resource does not exist.");
            responseDocument.Errors[0].Detail.Should().Be("Resource of type 'workItems' with ID '99999999' does not exist.");
        }

        [Fact]
        public async Task Cannot_create_on_unknown_relationship_in_url()
        {
            // Arrange
            var existingWorkItem = _fakers.WorkItem.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.WorkItems.Add(existingWorkItem);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "userAccounts",
                    id = 99999999
                }
            };

            var route = $"/workItems/{existingWorkItem.StringId}/relationships/doesNotExist";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.NotFound);
            responseDocument.Errors[0].Title.Should().Be("The requested relationship does not exist.");
            responseDocument.Errors[0].Detail.Should().Be("Resource of type 'workItems' does not contain a relationship named 'doesNotExist'.");
        }

        // TODO: Consider moving to RequestDeserializerTests
        [Fact]
        public async Task Cannot_create_on_relationship_mismatch_between_url_and_body()
        {
            // Arrange
            var existingWorkItem = _fakers.WorkItem.Generate();
            var existingColor = _fakers.RgbColor.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.AddRange(existingWorkItem, existingColor);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "rgbColors",
                    id = existingColor.StringId
                }
            };

            var route = $"/workItems/{existingWorkItem.StringId}/relationships/assignedTo";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePatchAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Conflict);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.Conflict);
            responseDocument.Errors[0].Title.Should().Be("Resource type mismatch between request body and endpoint URL.");
            responseDocument.Errors[0].Detail.Should().Be($"Expected resource of type 'userAccounts' in PATCH request body at endpoint '/workItems/{existingWorkItem.StringId}/relationships/assignedTo', instead of 'rgbColors'.");
        }
    }
}
