using System.Net;
using System.Reflection;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.ReadWrite.Creating;

public sealed class CreateResourceWithClientGeneratedIdTests : IClassFixture<IntegrationTestContext<TestableStartup<ReadWriteDbContext>, ReadWriteDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<ReadWriteDbContext>, ReadWriteDbContext> _testContext;
    private readonly ReadWriteFakers _fakers = new();

    public CreateResourceWithClientGeneratedIdTests(IntegrationTestContext<TestableStartup<ReadWriteDbContext>, ReadWriteDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<WorkItemGroupsController>();
        testContext.UseController<RgbColorsController>();
        testContext.UseController<UserAccountsController>();

        testContext.ConfigureServices(services =>
        {
            services.AddResourceDefinition<ImplicitlyChangingWorkItemGroupDefinition>();
            services.AddResourceDefinition<AssignIdToRgbColorDefinition>();
        });
    }

    [Theory]
    [InlineData(ClientIdGenerationMode.Allowed)]
    [InlineData(ClientIdGenerationMode.Required)]
    public async Task Can_create_resource_with_client_generated_guid_ID_having_side_effects(ClientIdGenerationMode mode)
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.ClientIdGeneration = mode;

        WorkItemGroup newGroup = _fakers.WorkItemGroup.Generate();
        newGroup.Id = Guid.NewGuid();

        var requestBody = new
        {
            data = new
            {
                type = "workItemGroups",
                id = newGroup.StringId,
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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        string groupName = $"{newGroup.Name}{ImplicitlyChangingWorkItemGroupDefinition.Suffix}";

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("workItemGroups");
        responseDocument.Data.SingleValue.Id.Should().Be(newGroup.StringId);
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("name").With(value => value.Should().Be(groupName));
        responseDocument.Data.SingleValue.Relationships.ShouldNotBeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WorkItemGroup groupInDatabase = await dbContext.Groups.FirstWithIdAsync(newGroup.Id);

            groupInDatabase.Name.Should().Be(groupName);
        });

        PropertyInfo? property = typeof(WorkItemGroup).GetProperty(nameof(Identifiable<object>.Id));
        property.ShouldNotBeNull();
        property.PropertyType.Should().Be(typeof(Guid));
    }

    [Theory]
    [InlineData(ClientIdGenerationMode.Allowed)]
    [InlineData(ClientIdGenerationMode.Required)]
    public async Task Can_create_resource_with_client_generated_guid_ID_having_side_effects_with_fieldset(ClientIdGenerationMode mode)
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.ClientIdGeneration = mode;

        WorkItemGroup newGroup = _fakers.WorkItemGroup.Generate();
        newGroup.Id = Guid.NewGuid();

        var requestBody = new
        {
            data = new
            {
                type = "workItemGroups",
                id = newGroup.StringId,
                attributes = new
                {
                    name = newGroup.Name
                }
            }
        };

        const string route = "/workItemGroups?fields[workItemGroups]=name";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        string groupName = $"{newGroup.Name}{ImplicitlyChangingWorkItemGroupDefinition.Suffix}";

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("workItemGroups");
        responseDocument.Data.SingleValue.Id.Should().Be(newGroup.StringId);
        responseDocument.Data.SingleValue.Attributes.ShouldHaveCount(1);
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("name").With(value => value.Should().Be(groupName));
        responseDocument.Data.SingleValue.Relationships.Should().BeNull();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            WorkItemGroup groupInDatabase = await dbContext.Groups.FirstWithIdAsync(newGroup.Id);

            groupInDatabase.Name.Should().Be(groupName);
        });

        PropertyInfo? property = typeof(WorkItemGroup).GetProperty(nameof(Identifiable<object>.Id));
        property.ShouldNotBeNull();
        property.PropertyType.Should().Be(typeof(Guid));
    }

    [Theory]
    [InlineData(ClientIdGenerationMode.Allowed)]
    [InlineData(ClientIdGenerationMode.Required)]
    public async Task Can_create_resource_with_client_generated_string_ID_having_no_side_effects(ClientIdGenerationMode mode)
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.ClientIdGeneration = mode;

        RgbColor newColor = _fakers.RgbColor.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<RgbColor>();
        });

        var requestBody = new
        {
            data = new
            {
                type = "rgbColors",
                id = newColor.StringId,
                attributes = new
                {
                    displayName = newColor.DisplayName
                }
            }
        };

        const string route = "/rgbColors";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            RgbColor colorInDatabase = await dbContext.RgbColors.FirstWithIdAsync(newColor.Id);

            colorInDatabase.DisplayName.Should().Be(newColor.DisplayName);
        });

        PropertyInfo? property = typeof(RgbColor).GetProperty(nameof(Identifiable<object>.Id));
        property.ShouldNotBeNull();
        property.PropertyType.Should().Be(typeof(string));
    }

    [Theory]
    [InlineData(ClientIdGenerationMode.Allowed)]
    [InlineData(ClientIdGenerationMode.Required)]
    public async Task Can_create_resource_with_client_generated_string_ID_having_no_side_effects_with_fieldset(ClientIdGenerationMode mode)
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.ClientIdGeneration = mode;

        RgbColor newColor = _fakers.RgbColor.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<RgbColor>();
        });

        var requestBody = new
        {
            data = new
            {
                type = "rgbColors",
                id = newColor.StringId,
                attributes = new
                {
                    displayName = newColor.DisplayName
                }
            }
        };

        const string route = "/rgbColors?fields[rgbColors]=id";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            RgbColor colorInDatabase = await dbContext.RgbColors.FirstWithIdAsync(newColor.Id);

            colorInDatabase.DisplayName.Should().Be(newColor.DisplayName);
        });

        PropertyInfo? property = typeof(RgbColor).GetProperty(nameof(Identifiable<object>.Id));
        property.ShouldNotBeNull();
        property.PropertyType.Should().Be(typeof(string));
    }

    [Theory]
    [InlineData(ClientIdGenerationMode.Allowed)]
    public async Task Can_create_resource_for_missing_client_generated_ID_having_side_effects(ClientIdGenerationMode mode)
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.ClientIdGeneration = mode;

        string newDisplayName = _fakers.RgbColor.Generate().DisplayName;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<RgbColor>();
        });

        var requestBody = new
        {
            data = new
            {
                type = "rgbColors",
                attributes = new
                {
                    displayName = newDisplayName
                }
            }
        };

        const string route = "/rgbColors";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        const string defaultId = AssignIdToRgbColorDefinition.DefaultId;
        const string defaultName = AssignIdToRgbColorDefinition.DefaultName;

        responseDocument.Data.SingleValue.ShouldNotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("rgbColors");
        responseDocument.Data.SingleValue.Id.Should().Be(defaultId);
        responseDocument.Data.SingleValue.Attributes.ShouldContainKey("displayName").With(value => value.Should().Be(defaultName));
        responseDocument.Data.SingleValue.Relationships.ShouldNotBeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            RgbColor colorInDatabase = await dbContext.RgbColors.FirstWithIdAsync((string?)defaultId);

            colorInDatabase.DisplayName.Should().Be(defaultName);
        });

        PropertyInfo? property = typeof(RgbColor).GetProperty(nameof(Identifiable<object>.Id));
        property.ShouldNotBeNull();
        property.PropertyType.Should().Be(typeof(string));
    }

    [Theory]
    [InlineData(ClientIdGenerationMode.Required)]
    public async Task Cannot_create_resource_for_missing_client_generated_ID(ClientIdGenerationMode mode)
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.ClientIdGeneration = mode;

        string newDisplayName = _fakers.RgbColor.Generate().DisplayName;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<RgbColor>();
        });

        var requestBody = new
        {
            data = new
            {
                type = "rgbColors",
                attributes = new
                {
                    displayName = newDisplayName
                }
            }
        };

        const string route = "/rgbColors";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: The 'id' element is required.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Theory]
    [InlineData(ClientIdGenerationMode.Allowed)]
    [InlineData(ClientIdGenerationMode.Required)]
    public async Task Cannot_create_resource_with_client_generated_zero_guid_ID(ClientIdGenerationMode mode)
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.ClientIdGeneration = mode;

        WorkItemGroup newGroup = _fakers.WorkItemGroup.Generate();

        var requestBody = new
        {
            data = new
            {
                type = "workItemGroups",
                id = Guid.Empty.ToString(),
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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: The 'id' element is invalid.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Theory]
    [InlineData(ClientIdGenerationMode.Allowed)]
    [InlineData(ClientIdGenerationMode.Required)]
    public async Task Cannot_create_resource_with_client_generated_empty_guid_ID(ClientIdGenerationMode mode)
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.ClientIdGeneration = mode;

        WorkItemGroup newGroup = _fakers.WorkItemGroup.Generate();

        var requestBody = new
        {
            data = new
            {
                type = "workItemGroups",
                id = string.Empty,
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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: The 'id' element is invalid.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Theory]
    [InlineData(ClientIdGenerationMode.Allowed)]
    [InlineData(ClientIdGenerationMode.Required)]
    public async Task Can_create_resource_with_client_generated_empty_string_ID(ClientIdGenerationMode mode)
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.ClientIdGeneration = mode;

        RgbColor newColor = _fakers.RgbColor.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<RgbColor>();
        });

        var requestBody = new
        {
            data = new
            {
                type = "rgbColors",
                id = string.Empty,
                attributes = new
                {
                    displayName = newColor.DisplayName
                }
            }
        };

        const string route = "/rgbColors?fields[rgbColors]=id";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            RgbColor colorInDatabase = await dbContext.RgbColors.FirstWithIdAsync((string?)string.Empty);

            colorInDatabase.DisplayName.Should().Be(newColor.DisplayName);
        });

        PropertyInfo? property = typeof(RgbColor).GetProperty(nameof(Identifiable<object>.Id));
        property.ShouldNotBeNull();
        property.PropertyType.Should().Be(typeof(string));
    }

    [Theory]
    [InlineData(ClientIdGenerationMode.Allowed)]
    [InlineData(ClientIdGenerationMode.Required)]
    public async Task Cannot_create_resource_with_client_generated_zero_long_ID(ClientIdGenerationMode mode)
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.ClientIdGeneration = mode;

        UserAccount newAccount = _fakers.UserAccount.Generate();

        var requestBody = new
        {
            data = new
            {
                type = "userAccounts",
                id = "0",
                attributes = new
                {
                    firstName = newAccount.FirstName,
                    lastName = newAccount.LastName
                }
            }
        };

        const string route = "/userAccounts";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: The 'id' element is invalid.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Theory]
    [InlineData(ClientIdGenerationMode.Allowed)]
    [InlineData(ClientIdGenerationMode.Required)]
    public async Task Cannot_create_resource_with_client_generated_empty_long_ID(ClientIdGenerationMode mode)
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.ClientIdGeneration = mode;

        UserAccount newAccount = _fakers.UserAccount.Generate();

        var requestBody = new
        {
            data = new
            {
                type = "userAccounts",
                id = string.Empty,
                attributes = new
                {
                    firstName = newAccount.FirstName,
                    lastName = newAccount.LastName
                }
            }
        };

        const string route = "/userAccounts";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body: The 'id' element is invalid.");
        error.Detail.Should().BeNull();
        error.Source.ShouldNotBeNull();
        error.Source.Pointer.Should().Be("/data");
        error.Meta.ShouldContainKey("requestBody").With(value => value.ShouldNotBeNull().ToString().ShouldNotBeEmpty());
    }

    [Theory]
    [InlineData(ClientIdGenerationMode.Allowed)]
    [InlineData(ClientIdGenerationMode.Required)]
    public async Task Cannot_create_resource_for_existing_client_generated_ID(ClientIdGenerationMode mode)
    {
        // Arrange
        var options = (JsonApiOptions)_testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
        options.ClientIdGeneration = mode;

        RgbColor existingColor = _fakers.RgbColor.Generate();

        RgbColor newColor = _fakers.RgbColor.Generate();
        newColor.Id = existingColor.Id;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<RgbColor>();
            dbContext.RgbColors.Add(existingColor);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "rgbColors",
                id = newColor.StringId,
                attributes = new
                {
                    displayName = newColor.DisplayName
                }
            }
        };

        const string route = "/rgbColors";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        error.Title.Should().Be("Another resource with the specified ID already exists.");
        error.Detail.Should().Be($"Another resource of type 'rgbColors' with ID '{existingColor.StringId}' already exists.");
        error.Source.Should().BeNull();
        error.Meta.Should().NotContainKey("requestBody");
    }
}
