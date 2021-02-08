using System;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExampleTests.Startups;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ReadWrite.Creating
{
    public sealed class CreateResourceWithClientGeneratedIdTests
        : IClassFixture<ExampleIntegrationTestContext<TestableStartup<ReadWriteDbContext>, ReadWriteDbContext>>
    {
        private readonly ExampleIntegrationTestContext<TestableStartup<ReadWriteDbContext>, ReadWriteDbContext> _testContext;
        private readonly ReadWriteFakers _fakers = new ReadWriteFakers();

        public CreateResourceWithClientGeneratedIdTests(ExampleIntegrationTestContext<TestableStartup<ReadWriteDbContext>, ReadWriteDbContext> testContext)
        {
            _testContext = testContext;

            var options = (JsonApiOptions) testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.AllowClientGeneratedIds = true;
        }

        [Fact]
        public async Task Can_create_resource_with_client_generated_guid_ID_having_side_effects()
        {
            // Arrange
            var newGroup = _fakers.WorkItemGroup.Generate();
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

            var route = "/workItemGroups";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Type.Should().Be("workItemGroups");
            responseDocument.SingleData.Id.Should().Be(newGroup.StringId);
            responseDocument.SingleData.Attributes["name"].Should().Be(newGroup.Name);
            responseDocument.SingleData.Relationships.Should().NotBeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var groupInDatabase = await dbContext.Groups
                    .FirstAsync(group => group.Id == newGroup.Id);

                groupInDatabase.Name.Should().Be(newGroup.Name);
            });

            var property = typeof(WorkItemGroup).GetProperty(nameof(Identifiable.Id));
            property.Should().NotBeNull().And.Subject.PropertyType.Should().Be(typeof(Guid));
        }

        [Fact]
        public async Task Can_create_resource_with_client_generated_guid_ID_having_side_effects_with_fieldset()
        {
            // Arrange
            var newGroup = _fakers.WorkItemGroup.Generate();
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

            var route = "/workItemGroups?fields[workItemGroups]=name";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Type.Should().Be("workItemGroups");
            responseDocument.SingleData.Id.Should().Be(newGroup.StringId);
            responseDocument.SingleData.Attributes.Should().HaveCount(1);
            responseDocument.SingleData.Attributes["name"].Should().Be(newGroup.Name);
            responseDocument.SingleData.Relationships.Should().BeNull();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var groupInDatabase = await dbContext.Groups
                    .FirstAsync(group => group.Id == newGroup.Id);

                groupInDatabase.Name.Should().Be(newGroup.Name);
            });

            var property = typeof(WorkItemGroup).GetProperty(nameof(Identifiable.Id));
            property.Should().NotBeNull().And.Subject.PropertyType.Should().Be(typeof(Guid));
        }

        [Fact]
        public async Task Can_create_resource_with_client_generated_string_ID_having_no_side_effects()
        {
            // Arrange
            var newColor = _fakers.RgbColor.Generate();
            
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

            var route = "/rgbColors";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var colorInDatabase = await dbContext.RgbColors
                    .FirstAsync(color => color.Id == newColor.Id);

                colorInDatabase.DisplayName.Should().Be(newColor.DisplayName);
            });

            var property = typeof(RgbColor).GetProperty(nameof(Identifiable.Id));
            property.Should().NotBeNull().And.Subject.PropertyType.Should().Be(typeof(string));
        }

        [Fact]
        public async Task Can_create_resource_with_client_generated_string_ID_having_no_side_effects_with_fieldset()
        {
            // Arrange
            var newColor = _fakers.RgbColor.Generate();
            
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

            var route = "/rgbColors?fields[rgbColors]=id";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);

            responseDocument.Should().BeEmpty();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                var colorInDatabase = await dbContext.RgbColors
                    .FirstAsync(color => color.Id == newColor.Id);

                colorInDatabase.DisplayName.Should().Be(newColor.DisplayName);
            });

            var property = typeof(RgbColor).GetProperty(nameof(Identifiable.Id));
            property.Should().NotBeNull().And.Subject.PropertyType.Should().Be(typeof(string));
        }

        [Fact]
        public async Task Cannot_create_resource_for_existing_client_generated_ID()
        {
            // Arrange
            var existingColor = _fakers.RgbColor.Generate();

            var colorToCreate = _fakers.RgbColor.Generate();
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

            var route = "/rgbColors";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecutePostAsync<ErrorDocument>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Conflict);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.Conflict);
            responseDocument.Errors[0].Title.Should().Be("Another resource with the specified ID already exists.");
            responseDocument.Errors[0].Detail.Should().Be($"Another resource of type 'rgbColors' with ID '{existingColor.StringId}' already exists.");
        }
    }
}
