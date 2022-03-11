using System.Net;
using System.Text.Json;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.ReadWrite.Creating;

public sealed class CreateResourceWithToOneRelationshipTests : IClassFixture<IntegrationTestContext<TestableStartup<ReadWriteDbContext>, ReadWriteDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<ReadWriteDbContext>, ReadWriteDbContext> _testContext;
    private readonly ReadWriteFakers _fakers = new();

    public CreateResourceWithToOneRelationshipTests(IntegrationTestContext<TestableStartup<ReadWriteDbContext>, ReadWriteDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<WorkItemGroupsController>();
        testContext.UseController<WorkItemsController>();
        testContext.UseController<RgbColorsController>();
        testContext.UseController<UserAccountsController>();

        var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.AllowClientGeneratedIds = true;
    }

    [Fact]
    public async Task Can_create_OneToOne_relationship_from_principal_side()
    {
        // Arrange
        WorkItemGroup existingGroup = _fakers.WorkItemGroup.Generate();
        existingGroup.Color = _fakers.RgbColor.Generate();

        string newGroupName = _fakers.WorkItemGroup.Generate().Name;

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
                attributes = new
                {
                    name = newGroupName
                },
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

        const string route = "/workItemGroups";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Attributes.ShouldNotBeEmpty();
        responseDocument.Data.SingleValue.Relationships.ShouldNotBeEmpty();

        string newGroupId = responseDocument.Data.SingleValue.Id.ShouldNotBeNull();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            List<WorkItemGroup> groupsInDatabase = await dbContext.Groups.Include(group => group.Color).ToListAsync();

            WorkItemGroup newGroupInDatabase = groupsInDatabase.Single(group => group.StringId == newGroupId);
            newGroupInDatabase.Name.Should().Be(newGroupName);
            newGroupInDatabase.Color.ShouldNotBeNull();
            newGroupInDatabase.Color.Id.Should().Be(existingGroup.Color.Id);

            WorkItemGroup existingGroupInDatabase = groupsInDatabase.Single(group => group.Id == existingGroup.Id);
            existingGroupInDatabase.Color.Should().BeNull();
        });
    }

    [Fact]
    public async Task Can_create_OneToOne_relationship_from_dependent_side()
    {
        // Arrange
        RgbColor existingColor = _fakers.RgbColor.Generate();
        existingColor.Group = _fakers.WorkItemGroup.Generate();

        const string newColorId = "0A0B0C";
        string newDisplayName = _fakers.RgbColor.Generate().DisplayName;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.RgbColors.Add(existingColor);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "rgbColors",
                id = newColorId,
                attributes = new
                {
                    displayName = newDisplayName
                },
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

        const string route = "/rgbColors";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            List<RgbColor> colorsInDatabase = await dbContext.RgbColors.Include(rgbColor => rgbColor.Group).ToListAsync();

            RgbColor newColorInDatabase = colorsInDatabase.Single(color => color.Id == newColorId);
            newColorInDatabase.DisplayName.Should().Be(newDisplayName);
            newColorInDatabase.Group.ShouldNotBeNull();
            newColorInDatabase.Group.Id.Should().Be(existingColor.Group.Id);

            RgbColor? existingColorInDatabase = colorsInDatabase.SingleOrDefault(color => color.Id == existingColor.Id);
            existingColorInDatabase.ShouldNotBeNull();
            existingColorInDatabase.Group.Should().BeNull();
        });
    }

    [Fact]
    public async Task Can_create_relationship_with_include()
    {
        // Arrange
        UserAccount existingUserAccount = _fakers.UserAccount.Generate();

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

        const string route = "/workItems?include=assignee";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Attributes.ShouldNotBeEmpty();
        responseDocument.Data.SingleValue.Relationships.ShouldNotBeEmpty();

        responseDocument.Included.ShouldHaveCount(1);
        responseDocument.Included[0].Type.Should().Be("userAccounts");
        responseDocument.Included[0].Id.Should().Be(existingUserAccount.StringId);
        responseDocument.Included[0].Attributes.ShouldContainKey("firstName").With(value => value.Should().Be(existingUserAccount.FirstName));
        responseDocument.Included[0].Attributes.ShouldContainKey("lastName").With(value => value.Should().Be(existingUserAccount.LastName));
        responseDocument.Included[0].Relationships.ShouldNotBeEmpty();

        int newWorkItemId = int.Parse(responseDocument.Data.SingleValue.Id.ShouldNotBeNull());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WorkItem workItemInDatabase = await dbContext.WorkItems.Include(workItem => workItem.Assignee).FirstWithIdAsync(newWorkItemId);

            workItemInDatabase.Assignee.ShouldNotBeNull();
            workItemInDatabase.Assignee.Id.Should().Be(existingUserAccount.Id);
        });
    }

    [Fact]
    public async Task Can_create_relationship_with_include_and_primary_fieldset()
    {
        // Arrange
        UserAccount existingUserAccount = _fakers.UserAccount.Generate();
        WorkItem newWorkItem = _fakers.WorkItem.Generate();

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

        const string route = "/workItems?fields[workItems]=description,assignee&include=assignee";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Attributes.ShouldHaveCount(1);
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("description").With(value => value.Should().Be(newWorkItem.Description));
        responseDocument.Data.SingleValue.Relationships.ShouldHaveCount(1);

        responseDocument.Data.SingleValue.Relationships.ShouldContainKey("assignee").With(value =>
        {
            value.ShouldNotBeNull();
            value.Data.SingleValue.ShouldNotBeNull();
            value.Data.SingleValue.Id.Should().Be(existingUserAccount.StringId);
        });

        responseDocument.Included.ShouldHaveCount(1);
        responseDocument.Included[0].Type.Should().Be("userAccounts");
        responseDocument.Included[0].Id.Should().Be(existingUserAccount.StringId);
        responseDocument.Included[0].Attributes.ShouldContainKey("firstName").With(value => value.Should().Be(existingUserAccount.FirstName));
        responseDocument.Included[0].Attributes.ShouldContainKey("lastName").With(value => value.Should().Be(existingUserAccount.LastName));
        responseDocument.Included[0].Relationships.ShouldNotBeEmpty();

        int newWorkItemId = int.Parse(responseDocument.Data.SingleValue.Id.ShouldNotBeNull());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WorkItem workItemInDatabase = await dbContext.WorkItems.Include(workItem => workItem.Assignee).FirstWithIdAsync(newWorkItemId);

            workItemInDatabase.Description.Should().Be(newWorkItem.Description);
            workItemInDatabase.Priority.Should().Be(newWorkItem.Priority);
            workItemInDatabase.Assignee.ShouldNotBeNull();
            workItemInDatabase.Assignee.Id.Should().Be(existingUserAccount.Id);
        });
    }

    [Fact]
    public async Task Cannot_create_with_null_relationship()
    {
        // Arrange
        UserAccount existingUserAccount = _fakers.UserAccount.Generate();

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
                    assignee = (object?)null
                }
            }
        };

        const string route = "/workItems";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Expected an object, instead of 'null'.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data/relationships/assignee");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Cannot_create_with_missing_data_in_relationship()
    {
        // Arrange
        UserAccount existingUserAccount = _fakers.UserAccount.Generate();

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
                    }
                }
            }
        };

        const string route = "/workItems";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: The 'data' element is required.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data/relationships/assignee");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Cannot_create_with_array_data_in_relationship()
    {
        // Arrange
        UserAccount existingUserAccount = _fakers.UserAccount.Generate();

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

        const string route = "/workItems";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Expected an object or 'null', instead of an array.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data/relationships/assignee/data");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
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
                            id = Unknown.StringId.For<UserAccount, long>()
                        }
                    }
                }
            }
        };

        const string route = "/workItems";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: The 'type' element is required.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data/relationships/assignee/data");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
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
                            type = Unknown.ResourceType,
                            id = Unknown.StringId.For<UserAccount, long>()
                        }
                    }
                }
            }
        };

        const string route = "/workItems";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Unknown resource type found.");
        error.Detail.Should().Be($"Resource type '{Unknown.ResourceType}' does not exist.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data/relationships/assignee/data/type");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
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

        const string route = "/workItems";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: The 'id' element is required.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data/relationships/assignee/data");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Cannot_create_with_unknown_relationship_ID()
    {
        // Arrange
        string userAccountId = Unknown.StringId.For<UserAccount, long>();

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
                            id = userAccountId
                        }
                    }
                }
            }
        };

        const string route = "/workItems";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("A related resource does not exist.");
        error.Detail.Should().Be($"Related resource of type 'userAccounts' with ID '{userAccountId}' in relationship 'assignee' does not exist.");
        error.Source.Should().BeNull();
        error.Meta.Should().NotContainKey("requestBody");
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

        const string route = "/workItems";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().Be("Failed to deserialize request body: Incompatible resource type found.");
        error.Detail.Should().Be("Type 'rgbColors' is incompatible with type 'userAccounts' of relationship 'assignee'.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data/relationships/assignee/data/type");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Can_create_resource_with_duplicate_relationship()
    {
        // Arrange
        List<UserAccount> existingUserAccounts = _fakers.UserAccount.Generate(2);

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

        string requestBodyText = JsonSerializer.Serialize(requestBody).Replace("assignee_duplicate", "assignee");

        const string route = "/workItems?include=assignee";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBodyText);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Attributes.ShouldNotBeEmpty();
        responseDocument.Data.SingleValue.Relationships.ShouldNotBeEmpty();

        responseDocument.Included.ShouldHaveCount(1);
        responseDocument.Included[0].Type.Should().Be("userAccounts");
        responseDocument.Included[0].Id.Should().Be(existingUserAccounts[1].StringId);
        responseDocument.Included[0].Attributes.ShouldContainKey("firstName").With(value => value.Should().Be(existingUserAccounts[1].FirstName));
        responseDocument.Included[0].Attributes.ShouldContainKey("lastName").With(value => value.Should().Be(existingUserAccounts[1].LastName));
        responseDocument.Included[0].Relationships.ShouldNotBeEmpty();

        int newWorkItemId = int.Parse(responseDocument.Data.SingleValue.Id.ShouldNotBeNull());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WorkItem workItemInDatabase = await dbContext.WorkItems.Include(workItem => workItem.Assignee).FirstWithIdAsync(newWorkItemId);

            workItemInDatabase.Assignee.ShouldNotBeNull();
            workItemInDatabase.Assignee.Id.Should().Be(existingUserAccounts[1].Id);
        });
    }

    [Fact]
    public async Task Cannot_create_resource_with_local_ID()
    {
        // Arrange
        const string workItemLocalId = "wo-1";

        var requestBody = new
        {
            data = new
            {
                type = "workItems",
                lid = workItemLocalId,
                relationships = new
                {
                    parent = new
                    {
                        data = new
                        {
                            type = "workItems",
                            lid = workItemLocalId
                        }
                    }
                }
            }
        };

        const string route = "/workItems";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: The 'lid' element is not supported at this endpoint.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data/lid");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }
}
