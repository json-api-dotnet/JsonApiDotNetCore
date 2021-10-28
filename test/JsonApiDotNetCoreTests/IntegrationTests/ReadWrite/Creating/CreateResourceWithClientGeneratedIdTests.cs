using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.ReadWrite.Creating
{
    public sealed class CreateResourceWithClientGeneratedIdTests
        : IClassFixture<IntegrationTestContext<TestableStartup<ReadWriteDbContext>, ReadWriteDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<ReadWriteDbContext>, ReadWriteDbContext> _testContext;
        private readonly ReadWriteFakers _fakers = new();

        public CreateResourceWithClientGeneratedIdTests(IntegrationTestContext<TestableStartup<ReadWriteDbContext>, ReadWriteDbContext> testContext)
        {
            _testContext = testContext;

            testContext.UseController<WorkItemGroupsController>();
            testContext.UseController<RgbColorsController>();

            testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddResourceDefinition<ImplicitlyChangingWorkItemGroupDefinition>();
            });

            var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.AllowClientGeneratedIds = true;
        }

        [Fact]
        public async Task Can_create_resource_with_client_generated_guid_ID_having_side_effects()
        {
            // Arrange
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
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

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

        [Fact]
        public async Task Can_create_resource_with_client_generated_guid_ID_having_side_effects_with_fieldset()
        {
            // Arrange
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
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

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

        [Fact]
        public async Task Can_create_resource_with_client_generated_string_ID_having_no_side_effects()
        {
            // Arrange
            RgbColor newColor = _fakers.RgbColor.Generate();

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
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

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

        [Fact]
        public async Task Can_create_resource_with_client_generated_string_ID_having_no_side_effects_with_fieldset()
        {
            // Arrange
            RgbColor newColor = _fakers.RgbColor.Generate();

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
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

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

        [Fact]
        public async Task Cannot_create_resource_for_existing_client_generated_ID()
        {
            // Arrange
            RgbColor existingColor = _fakers.RgbColor.Generate();

            RgbColor colorToCreate = _fakers.RgbColor.Generate();
            colorToCreate.Id = existingColor.Id;

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
                    id = colorToCreate.StringId,
                    attributes = new
                    {
                        displayName = colorToCreate.DisplayName
                    }
                }
            };

            const string route = "/rgbColors";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Conflict);

            responseDocument.Errors.ShouldHaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.Conflict);
            error.Title.Should().Be("Another resource with the specified ID already exists.");
            error.Detail.Should().Be($"Another resource of type 'rgbColors' with ID '{existingColor.StringId}' already exists.");
            error.Source.Should().BeNull();
            error.Meta.Should().NotContainKey("requestBody");
        }
    }
}
