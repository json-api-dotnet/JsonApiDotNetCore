using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ReadWrite.Creating
{
    public sealed class CreateResourceWithToOneRelationshipTests
        : IClassFixture<IntegrationTestContext<TestableStartup<WriteDbContext>, WriteDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<WriteDbContext>, WriteDbContext> _testContext;
        private readonly WriteFakers _fakers = new WriteFakers();

        public CreateResourceWithToOneRelationshipTests(IntegrationTestContext<TestableStartup<WriteDbContext>, WriteDbContext> testContext)
        {
            _testContext = testContext;

            var options = (JsonApiOptions) testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.AllowClientGeneratedIds = true;
        }

        [Fact]
        public async Task Can_create_OneToOne_relationship_from_principal_side()
        {
            // Arrange
            var existingGroup = _fakers.WorkItemGroup.Generate();
            existingGroup.Color = _fakers.RgbColor.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Groups.Add(existingGroup);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "workItemGroups",
                    relationships = new
                    {
                        color = new
                        {
                            data = new
                            {
                                type = "rgbColors",
                                id = existingGroup.Color.StringId
                            }
                        }
                    }
                }
            };

            var route = "/workItemGroups";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Relationships.Should().NotBeEmpty();

            var newGroupId = responseDocument.SingleData.Id;
            newGroupId.Should().NotBeNullOrEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var groupsInDatabase = await dbContext.Groups
                    .Include(group => group.Color)
                    .ToListAsync();

                var newGroupInDatabase = groupsInDatabase.Single(group => group.StringId == newGroupId);
                newGroupInDatabase.Color.Should().NotBeNull();
                newGroupInDatabase.Color.Id.Should().Be(existingGroup.Color.Id);

                var existingGroupInDatabase = groupsInDatabase.Single(group => group.Id == existingGroup.Id);
                existingGroupInDatabase.Color.Should().BeNull();
            });
        }

        [Fact]
        public async Task Can_create_OneToOne_relationship_from_dependent_side()
        {
            // Arrange
            var existingColor = _fakers.RgbColor.Generate();
            existingColor.Group = _fakers.WorkItemGroup.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.RgbColors.Add(existingColor);
                await dbContext.SaveChangesAsync();
            });

            string colorId = "0A0B0C";

            var requestBody = new
            {
                data = new
                {
                    type = "rgbColors",
                    id = colorId,
                    relationships = new
                    {
                        group = new
                        {
                            data = new
                            {
                                type = "workItemGroups",
                                id = existingColor.Group.StringId
                            }
                        }
                    }
                }
            };

            var route = "/rgbColors";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var colorsInDatabase = await dbContext.RgbColors
                    .Include(rgbColor => rgbColor.Group)
                    .ToListAsync();

                var newColorInDatabase = colorsInDatabase.Single(color => color.Id == colorId);
                newColorInDatabase.Group.Should().NotBeNull();
                newColorInDatabase.Group.Id.Should().Be(existingColor.Group.Id);

                var existingColorInDatabase = colorsInDatabase.SingleOrDefault(color => color.Id == existingColor.Id);
                existingColorInDatabase.Should().NotBeNull();
                existingColorInDatabase!.Group.Should().BeNull();
            });
        }

        [Fact]
        public async Task Can_create_relationship_with_include()
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
                    type = "workItems",
                    relationships = new
                    {
                        assignee = new
                        {
                            data = new
                            {
                                type = "userAccounts",
                                id = existingUserAccount.StringId
                            }
                        }
                    }
                }
            };

            var route = "/workItems?include=assignee";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Relationships.Should().NotBeEmpty();

            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Type.Should().Be("userAccounts");
            responseDocument.Included[0].Id.Should().Be(existingUserAccount.StringId);
            responseDocument.Included[0].Attributes["firstName"].Should().Be(existingUserAccount.FirstName);
            responseDocument.Included[0].Attributes["lastName"].Should().Be(existingUserAccount.LastName);

            var newWorkItemId = int.Parse(responseDocument.SingleData.Id);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var workItemInDatabase = await dbContext.WorkItems
                    .Include(workItem => workItem.Assignee)
                    .FirstAsync(workItem => workItem.Id == newWorkItemId);

                workItemInDatabase.Assignee.Should().NotBeNull();
                workItemInDatabase.Assignee.Id.Should().Be(existingUserAccount.Id);
            });
        }

        [Fact]
        public async Task Can_create_relationship_with_include_and_primary_fieldset()
        {
            // Arrange
            var existingUserAccount = _fakers.UserAccount.Generate();
            var newWorkItem = _fakers.WorkItem.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.UserAccounts.Add(existingUserAccount);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    attributes = new
                    {
                        description = newWorkItem.Description,
                        priority = newWorkItem.Priority
                    },
                    relationships = new
                    {
                        assignee = new
                        {
                            data = new
                            {
                                type = "userAccounts",
                                id = existingUserAccount.StringId
                            }
                        }
                    }
                }
            };

            var route = "/workItems?fields=description&include=assignee";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Attributes.Should().HaveCount(1);
            responseDocument.SingleData.Attributes["description"].Should().Be(newWorkItem.Description);

            responseDocument.SingleData.Relationships.Should().NotBeEmpty();

            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Type.Should().Be("userAccounts");
            responseDocument.Included[0].Id.Should().Be(existingUserAccount.StringId);
            responseDocument.Included[0].Attributes["firstName"].Should().Be(existingUserAccount.FirstName);
            responseDocument.Included[0].Attributes["lastName"].Should().Be(existingUserAccount.LastName);

            var newWorkItemId = int.Parse(responseDocument.SingleData.Id);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var workItemInDatabase = await dbContext.WorkItems
                    .Include(workItem => workItem.Assignee)
                    .FirstAsync(workItem => workItem.Id == newWorkItemId);

                workItemInDatabase.Description.Should().Be(newWorkItem.Description);
                workItemInDatabase.Priority.Should().Be(newWorkItem.Priority);
                workItemInDatabase.Assignee.Should().NotBeNull();
                workItemInDatabase.Assignee.Id.Should().Be(existingUserAccount.Id);
            });
        }

        [Fact]
        public async Task Cannot_create_for_missing_relationship_type()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    relationships = new
                    {
                        assignee = new
                        {
                            data = new
                            {
                                id = 12345678
                            }
                        }
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
            responseDocument.Errors[0].Detail.Should().StartWith("Expected 'type' element in 'assignee' relationship. - Request body: <<");
        }

        [Fact]
        public async Task Cannot_create_for_unknown_relationship_type()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    relationships = new
                    {
                        assignee = new
                        {
                            data = new
                            {
                                type = "doesNotExist",
                                id = 12345678
                            }
                        }
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
            responseDocument.Errors[0].Detail.Should().StartWith("Resource type 'doesNotExist' does not exist. - Request body: <<");
        }

        [Fact]
        public async Task Cannot_create_for_missing_relationship_ID()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    relationships = new
                    {
                        assignee = new
                        {
                            data = new
                            {
                                type = "userAccounts"
                            }
                        }
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
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: Request body must include 'id' element.");
            responseDocument.Errors[0].Detail.Should().StartWith("Expected 'id' element in 'assignee' relationship. - Request body: <<");
        }

        [Fact]
        public async Task Cannot_create_with_unknown_relationship_ID()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    relationships = new
                    {
                        assignee = new
                        {
                            data = new
                            {
                                type = "userAccounts",
                                id = 12345678
                            }
                        }
                    }
                }
            };

            var route = "/workItems";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.NotFound);
            responseDocument.Errors[0].Title.Should().Be("A related resource does not exist.");
            responseDocument.Errors[0].Detail.Should().StartWith("Related resource of type 'userAccounts' with ID '12345678' in relationship 'assignee' does not exist.");
        }

        [Fact]
        public async Task Cannot_create_on_relationship_type_mismatch()
        {
            // Arrange
            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    relationships = new
                    {
                        assignee = new
                        {
                            data = new
                            {
                                type = "rgbColors",
                                id = "0A0B0C"
                            }
                        }
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
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: Relationship contains incompatible resource type.");
            responseDocument.Errors[0].Detail.Should().StartWith("Relationship 'assignee' contains incompatible resource type 'rgbColors'. - Request body: <<");
        }

        [Fact]
        public async Task Can_create_resource_with_duplicate_relationship()
        {
            // Arrange
            var existingUserAccounts = _fakers.UserAccount.Generate(2);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.UserAccounts.AddRange(existingUserAccounts);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "workItems",
                    relationships = new
                    {
                        assignee = new
                        {
                            data = new
                            {
                                type = "userAccounts",
                                id = existingUserAccounts[0].StringId
                            }
                        },
                        assignee_duplicate = new
                        {
                            data = new
                            {
                                type = "userAccounts",
                                id = existingUserAccounts[1].StringId
                            }
                        }
                    }
                }
            };

            var requestBodyText = JsonConvert.SerializeObject(requestBody).Replace("assignee_duplicate", "assignee");

            var route = "/workItems?include=assignee";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBodyText);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Relationships.Should().NotBeEmpty();

            responseDocument.Included.Should().HaveCount(1);
            responseDocument.Included[0].Type.Should().Be("userAccounts");
            responseDocument.Included[0].Id.Should().Be(existingUserAccounts[1].StringId);
            responseDocument.Included[0].Attributes["firstName"].Should().Be(existingUserAccounts[1].FirstName);
            responseDocument.Included[0].Attributes["lastName"].Should().Be(existingUserAccounts[1].LastName);

            var newWorkItemId = int.Parse(responseDocument.SingleData.Id);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var workItemInDatabase = await dbContext.WorkItems
                    .Include(workItem => workItem.Assignee)
                    .FirstAsync(workItem => workItem.Id == newWorkItemId);

                workItemInDatabase.Assignee.Should().NotBeNull();
                workItemInDatabase.Assignee.Id.Should().Be(existingUserAccounts[1].Id);
            });
        }

        [Fact]
        public async Task Cannot_create_with_data_array_in_relationship()
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
                    type = "workItems",
                    relationships = new
                    {
                        assignee = new
                        {
                            data = new[]
                            {
                                new
                                {
                                    type = "userAccounts",
                                    id = existingUserAccount.StringId
                                }
                            }
                        }
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
            responseDocument.Errors[0].Title.Should().Be("Failed to deserialize request body: Expected single data element for to-one relationship.");
            responseDocument.Errors[0].Detail.Should().StartWith("Expected single data element for 'assignee' relationship. - Request body: <<");
        }
    }
}
