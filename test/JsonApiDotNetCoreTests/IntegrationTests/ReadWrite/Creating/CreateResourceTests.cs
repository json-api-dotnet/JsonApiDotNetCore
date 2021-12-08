using System.Net;
using System.Reflection;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.ReadWrite.Creating;

public sealed class CreateResourceTests : IClassFixture<IntegrationTestContext<TestableStartup<ReadWriteDbContext>, ReadWriteDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<ReadWriteDbContext>, ReadWriteDbContext> _testContext;
    private readonly ReadWriteFakers _fakers = new();

    public CreateResourceTests(IntegrationTestContext<TestableStartup<ReadWriteDbContext>, ReadWriteDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<WorkItemsController>();
        testContext.UseController<WorkItemGroupsController>();
        testContext.UseController<UserAccountsController>();
        testContext.UseController<RgbColorsController>();

        var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.UseRelativeLinks = false;
        options.AllowClientGeneratedIds = false;
        options.AllowUnknownFieldsInRequestBody = false;
    }

    [Fact]
    public async Task Sets_location_header_for_created_resource()
    {
        // Arrange
        WorkItem newWorkItem = _fakers.WorkItem.Generate();

        var requestBody = new
        {
            data = new
            {
                type = "workItems",
                attributes = new
                {
                    description = newWorkItem.Description
                }
            }
        };

        const string route = "/workItems";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

        string newWorkItemId = responseDocument.Data.SingleValue.ShouldNotBeNull().Id.ShouldNotBeNull();
        httpResponse.Headers.Location.Should().Be($"/workItems/{newWorkItemId}");

        responseDocument.Links.ShouldNotBeNull();
        responseDocument.Links.Self.Should().Be("http://localhost/workItems");
        responseDocument.Links.First.Should().BeNull();

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Links.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Links.Self.Should().Be($"http://localhost{httpResponse.Headers.Location}");
    }

    [Fact]
    public async Task Can_create_resource_with_int_ID()
    {
        // Arrange
        WorkItem newWorkItem = _fakers.WorkItem.Generate();
        newWorkItem.DueAt = null;

        var requestBody = new
        {
            data = new
            {
                type = "workItems",
                attributes = new
                {
                    description = newWorkItem.Description
                }
            }
        };

        const string route = "/workItems";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("workItems");
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("description").With(value => value.Should().Be(newWorkItem.Description));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("dueAt").With(value => value.Should().Be(newWorkItem.DueAt));
        responseDocument.Data.SingleValue.Relationships.ShouldNotBeEmpty();

        int newWorkItemId = int.Parse(responseDocument.Data.SingleValue.Id.ShouldNotBeNull());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WorkItem workItemInDatabase = await dbContext.WorkItems.FirstWithIdAsync(newWorkItemId);

            workItemInDatabase.Description.Should().Be(newWorkItem.Description);
            workItemInDatabase.DueAt.Should().Be(newWorkItem.DueAt);
        });

        PropertyInfo? property = typeof(WorkItem).GetProperty(nameof(Identifiable<object>.Id));
        property.ShouldNotBeNull();
        property.PropertyType.Should().Be(typeof(int));
    }

    [Fact]
    public async Task Can_create_resource_with_long_ID()
    {
        // Arrange
        UserAccount newUserAccount = _fakers.UserAccount.Generate();

        var requestBody = new
        {
            data = new
            {
                type = "userAccounts",
                attributes = new
                {
                    firstName = newUserAccount.FirstName,
                    lastName = newUserAccount.LastName
                }
            }
        };

        const string route = "/userAccounts";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("userAccounts");
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("firstName").With(value => value.Should().Be(newUserAccount.FirstName));
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("lastName").With(value => value.Should().Be(newUserAccount.LastName));
        responseDocument.Data.SingleValue.Relationships.ShouldNotBeEmpty();

        long newUserAccountId = long.Parse(responseDocument.Data.SingleValue.Id.ShouldNotBeNull());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            UserAccount userAccountInDatabase = await dbContext.UserAccounts.FirstWithIdAsync(newUserAccountId);

            userAccountInDatabase.FirstName.Should().Be(newUserAccount.FirstName);
            userAccountInDatabase.LastName.Should().Be(newUserAccount.LastName);
        });

        PropertyInfo? property = typeof(UserAccount).GetProperty(nameof(Identifiable<object>.Id));
        property.ShouldNotBeNull();
        property.PropertyType.Should().Be(typeof(long));
    }

    [Fact]
    public async Task Can_create_resource_with_guid_ID()
    {
        // Arrange
        WorkItemGroup newGroup = _fakers.WorkItemGroup.Generate();

        var requestBody = new
        {
            data = new
            {
                type = "workItemGroups",
                attributes = new
                {
                    name = newGroup.Name
                }
            }
        };

        const string route = "/workItemGroups";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("workItemGroups");
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("name").With(value => value.Should().Be(newGroup.Name));
        responseDocument.Data.SingleValue.Relationships.ShouldNotBeEmpty();

        Guid newGroupId = Guid.Parse(responseDocument.Data.SingleValue.Id.ShouldNotBeNull());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WorkItemGroup groupInDatabase = await dbContext.Groups.FirstWithIdAsync(newGroupId);

            groupInDatabase.Name.Should().Be(newGroup.Name);
        });

        PropertyInfo? property = typeof(WorkItemGroup).GetProperty(nameof(Identifiable<object>.Id));
        property.ShouldNotBeNull();
        property.PropertyType.Should().Be(typeof(Guid));
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

        const string route = "/workItems";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("workItems");
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("description").With(value => value.Should().BeNull());
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("dueAt").With(value => value.Should().BeNull());
        responseDocument.Data.SingleValue.Relationships.ShouldNotBeEmpty();

        int newWorkItemId = int.Parse(responseDocument.Data.SingleValue.Id.ShouldNotBeNull());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WorkItem workItemInDatabase = await dbContext.WorkItems.FirstWithIdAsync(newWorkItemId);

            workItemInDatabase.Description.Should().BeNull();
            workItemInDatabase.DueAt.Should().BeNull();
        });
    }

    [Fact]
    public async Task Cannot_create_resource_with_unknown_attribute()
    {
        // Arrange
        WorkItem newWorkItem = _fakers.WorkItem.Generate();

        var requestBody = new
        {
            data = new
            {
                type = "workItems",
                attributes = new
                {
                    doesNotExist = "ignored",
                    description = newWorkItem.Description
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
        error.Title.Should().Be("Failed to deserialize request body: Unknown attribute found.");
        error.Detail.Should().Be("Attribute 'doesNotExist' does not exist on resource type 'workItems'.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/doesNotExist");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Can_create_resource_with_unknown_attribute()
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.AllowUnknownFieldsInRequestBody = true;

        WorkItem newWorkItem = _fakers.WorkItem.Generate();

        var requestBody = new
        {
            data = new
            {
                type = "workItems",
                attributes = new
                {
                    doesNotExist = "ignored",
                    description = newWorkItem.Description
                }
            }
        };

        const string route = "/workItems";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("workItems");
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("description").With(value => value.Should().Be(newWorkItem.Description));
        responseDocument.Data.SingleValue.Relationships.ShouldNotBeEmpty();

        int newWorkItemId = int.Parse(responseDocument.Data.SingleValue.Id.ShouldNotBeNull());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WorkItem workItemInDatabase = await dbContext.WorkItems.FirstWithIdAsync(newWorkItemId);

            workItemInDatabase.Description.Should().Be(newWorkItem.Description);
        });
    }

    [Fact]
    public async Task Cannot_create_resource_with_unknown_relationship()
    {
        // Arrange
        var requestBody = new
        {
            data = new
            {
                type = "workItems",
                relationships = new
                {
                    doesNotExist = new
                    {
                        data = new
                        {
                            type = Unknown.ResourceType,
                            id = Unknown.StringId.Int32
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
        error.Title.Should().Be("Failed to deserialize request body: Unknown relationship found.");
        error.Detail.Should().Be("Relationship 'doesNotExist' does not exist on resource type 'workItems'.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data/relationships/doesNotExist");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Can_create_resource_with_unknown_relationship()
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.AllowUnknownFieldsInRequestBody = true;

        var requestBody = new
        {
            data = new
            {
                type = "workItems",
                relationships = new
                {
                    doesNotExist = new
                    {
                        data = new
                        {
                            type = Unknown.ResourceType,
                            id = Unknown.StringId.Int32
                        }
                    }
                }
            }
        };

        const string route = "/workItems";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("workItems");
        responseDocument.Data.SingleValue.Attributes.ShouldNotBeEmpty();
        responseDocument.Data.SingleValue.Relationships.ShouldNotBeEmpty();

        int newWorkItemId = int.Parse(responseDocument.Data.SingleValue.Id.ShouldNotBeNull());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WorkItem? workItemInDatabase = await dbContext.WorkItems.FirstWithIdOrDefaultAsync(newWorkItemId);

            workItemInDatabase.ShouldNotBeNull();
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
                id = "0A0B0C",
                attributes = new
                {
                    displayName = "Black"
                }
            }
        };

        const string route = "/rgbColors";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.Forbidden);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        error.Title.Should().Be("Failed to deserialize request body: The use of client-generated IDs is disabled.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data/id");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Cannot_create_resource_for_missing_request_body()
    {
        // Arrange
        string requestBody = string.Empty;

        const string route = "/workItems";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Failed to deserialize request body: Missing request body.");
        error.Detail.Should().BeNull();
        error.Source.Should().BeNull();
        error.Meta.Should().NotContainKey("requestBody");
    }

    [Fact]
    public async Task Cannot_create_resource_for_null_request_body()
    {
        // Arrange
        const string requestBody = "null";

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
        error.Source.Should().BeNull();
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Cannot_create_resource_for_missing_data()
    {
        // Arrange
        var requestBody = new
        {
            meta = new
            {
                key = "value"
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
        error.Source.Should().BeNull();
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Cannot_create_resource_for_null_data()
    {
        // Arrange
        var requestBody = new
        {
            data = (object?)null
        };

        const string route = "/workItems";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Expected an object, instead of 'null'.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Cannot_create_resource_for_array_data()
    {
        // Arrange
        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "workItems"
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
        error.Title.Should().Be("Failed to deserialize request body: Expected an object, instead of an array.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
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
        error.Source.Pointer.Should().Be("/data");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Cannot_create_resource_for_unknown_type()
    {
        // Arrange
        var requestBody = new
        {
            data = new
            {
                type = Unknown.ResourceType,
                attributes = new
                {
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
        error.Source.Pointer.Should().Be("/data/type");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
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

        const string route = "/" + Unknown.ResourceType;

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Should().BeEmpty();
    }

    [Fact]
    public async Task Cannot_create_on_resource_type_mismatch_between_url_and_body()
    {
        // Arrange
        var requestBody = new
        {
            data = new
            {
                type = "rgbColors",
                id = "0A0B0C"
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
        error.Detail.Should().Be("Type 'rgbColors' is incompatible with type 'workItems'.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data/type");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Cannot_create_resource_attribute_with_blocked_capability()
    {
        // Arrange
        var requestBody = new
        {
            data = new
            {
                type = "workItems",
                attributes = new
                {
                    isImportant = true
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
        error.Title.Should().Be("Failed to deserialize request body: Attribute value cannot be assigned when creating resource.");
        error.Detail.Should().Be("The attribute 'isImportant' on resource type 'workItems' cannot be assigned to.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/isImportant");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
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
                    isDeprecated = false
                }
            }
        };

        const string route = "/workItemGroups";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: Attribute is read-only.");
        error.Detail.Should().Be("Attribute 'isDeprecated' on resource type 'workItemGroups' is read-only.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/isDeprecated");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Cannot_create_resource_for_broken_JSON_request_body()
    {
        // Arrange
        const string requestBody = "{ \"data\" {";

        const string route = "/workItemGroups";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body.");
        error.Detail.Should().StartWith("'{' is invalid after a property name.");
        error.Source.Should().BeNull();
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Cannot_create_resource_with_incompatible_attribute_value()
    {
        // Arrange
        var requestBody = new
        {
            data = new
            {
                type = "workItems",
                attributes = new
                {
                    dueAt = "not-a-valid-time"
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
        error.Title.Should().Be("Failed to deserialize request body: Incompatible attribute value found.");
        error.Detail.Should().Be("Failed to convert attribute 'dueAt' with value 'not-a-valid-time' of type 'String' to type 'Nullable<DateTimeOffset>'.");
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/dueAt");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Can_create_resource_with_attributes_and_multiple_relationship_types()
    {
        // Arrange
        List<UserAccount> existingUserAccounts = _fakers.UserAccount.Generate(2);
        WorkTag existingTag = _fakers.WorkTag.Generate();

        string newDescription = _fakers.WorkItem.Generate().Description!;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.UserAccounts.AddRange(existingUserAccounts);
            dbContext.WorkTags.Add(existingTag);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "workItems",
                attributes = new
                {
                    description = newDescription
                },
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
                    subscribers = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "userAccounts",
                                id = existingUserAccounts[1].StringId
                            }
                        }
                    },
                    tags = new
                    {
                        data = new[]
                        {
                            new
                            {
                                type = "workTags",
                                id = existingTag.StringId
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
        httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("description").With(value => value.Should().Be(newDescription));
        responseDocument.Data.SingleValue.Relationships.ShouldNotBeEmpty();

        int newWorkItemId = int.Parse(responseDocument.Data.SingleValue.Id.ShouldNotBeNull());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:keep_existing_linebreaks true

            WorkItem workItemInDatabase = await dbContext.WorkItems
                .Include(workItem => workItem.Assignee)
                .Include(workItem => workItem.Subscribers)
                .Include(workItem => workItem.Tags)
                .FirstWithIdAsync(newWorkItemId);

            // @formatter:keep_existing_linebreaks restore
            // @formatter:wrap_chained_method_calls restore

            workItemInDatabase.Description.Should().Be(newDescription);

            workItemInDatabase.Assignee.ShouldNotBeNull();
            workItemInDatabase.Assignee.Id.Should().Be(existingUserAccounts[0].Id);

            workItemInDatabase.Subscribers.ShouldHaveCount(1);
            workItemInDatabase.Subscribers.Single().Id.Should().Be(existingUserAccounts[1].Id);

            workItemInDatabase.Tags.ShouldHaveCount(1);
            workItemInDatabase.Tags.Single().Id.Should().Be(existingTag.Id);
        });
    }
}
